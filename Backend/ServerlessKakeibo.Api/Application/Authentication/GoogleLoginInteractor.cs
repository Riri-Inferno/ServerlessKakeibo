using Google.Apis.Auth;
using ServerlessKakeibo.Api.Application.Authentication.Dto;
using ServerlessKakeibo.Api.Application.Authentication.Usecases;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Service.Interface;

namespace ServerlessKakeibo.Api.Application.Authentication;

/// <summary>
/// Google認証ログインインタラクター
/// </summary>
public class GoogleLoginInteractor : IGoogleLoginUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IGenericReadRepository<UserEntity> _userReadRepository;
    private readonly IGenericWriteRepository<UserEntity> _userWriteRepository;
    private readonly IUserExternalLoginRepository _externalLoginRepository;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleLoginInteractor> _logger;

    public GoogleLoginInteractor(
        ITransactionHelper transactionHelper,
        IGenericReadRepository<UserEntity> userReadRepository,
        IGenericWriteRepository<UserEntity> userWriteRepository,
        IUserExternalLoginRepository externalLoginRepository,
        IJwtTokenService jwtService,
        IPasswordHashService passwordHashService,
        IConfiguration configuration,
        ILogger<GoogleLoginInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        _userWriteRepository = userWriteRepository ?? throw new ArgumentNullException(nameof(userWriteRepository));
        _externalLoginRepository = externalLoginRepository ?? throw new ArgumentNullException(nameof(externalLoginRepository));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _passwordHashService = passwordHashService ?? throw new ArgumentNullException(nameof(passwordHashService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Google認証でログイン
    /// </summary>
    /// <param name="idToken">GoogleのIDトークン</param>
    /// <param name="cancellationToken"></param>
    /// <returns>ログイン結果(JWTトークン含む)</returns>
    public async Task<LoginResult> ExecuteAsync(
        string idToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idToken))
            throw new ArgumentException("IDトークンが指定されていません", nameof(idToken));

        try
        {
            _logger.LogInformation("Google認証ログイン処理を開始します");

            return await _transactionHelper.ExecuteInTransactionAsync(async () =>
            {
                // 1. Googleトークンを検証
                var payload = await ValidateGoogleTokenAsync(idToken, cancellationToken);

                _logger.LogDebug(
                    "Googleトークンを検証しました。Email: {Email}, Subject: {Subject}",
                    payload.Email, payload.Subject);

                // 2. 既存ユーザーを確認
                var externalLogin = await _externalLoginRepository
                    .GetByProviderAsync(AuthProvider.Google, payload.Subject, cancellationToken);

                UserEntity user;

                if (externalLogin == null)
                {
                    // 3. 新規ユーザー作成
                    user = await CreateNewUserAsync(payload, cancellationToken);

                    _logger.LogInformation(
                        "新規ユーザーを作成しました。UserId: {UserId}, Email: {Email}",
                        user.Id, user.Email);
                }
                else
                {
                    // 4. 既存ユーザー取得
                    var retrievedUser = await _userReadRepository.GetByIdAsync(
                        externalLogin.UserId, cancellationToken);

                    if (retrievedUser == null)
                    {
                        _logger.LogError(
                            "外部ログイン情報は存在するが、ユーザーが見つかりません。UserId: {UserId}",
                            externalLogin.UserId);
                        throw new InvalidOperationException("ユーザー情報が見つかりません");
                    }

                    user = retrievedUser;

                    // プロフィール情報を更新
                    await UpdateUserProfileAsync(user, payload, cancellationToken);

                    _logger.LogInformation(
                        "既存ユーザーでログインしました。UserId: {UserId}, Email: {Email}",
                        user.Id, user.Email);
                }

                // 5. JWTトークンを生成
                var accessToken = _jwtService.GenerateToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                // 6. RefreshToken をハッシュ化して DB に保存
                user.RefreshTokenHash = _passwordHashService.HashPassword(refreshToken);
                user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(_jwtService.RefreshTokenExpirationDays);
                user.UpdatedAt = DateTimeOffset.UtcNow;
                user.UpdatedBy = user.Id;
                await _userWriteRepository.UpdateAsync(user, cancellationToken);

                _logger.LogInformation(
                    "JWTトークンを発行しました。UserId: {UserId}, RefreshTokenExpiry: {Expiry}",
                    user.Id, user.RefreshTokenExpiry);

                // 7. クライアントには平文のトークンを返す
                return new LoginResult(
                    accessToken,
                    refreshToken, // ← 平文で返す
                    user.Id,
                    user.DisplayName,
                    user.PictureUrl
                );
            });
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "無効なGoogleトークンです");
            throw new UnauthorizedAccessException("Googleトークンの検証に失敗しました", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google認証ログイン中にエラーが発生しました");
            throw;
        }
    }

    #region private methods

    /// <summary>
    /// GoogleのIDトークンを検証
    /// </summary>
    private async Task<GoogleJsonWebSignature.Payload> ValidateGoogleTokenAsync(
        string idToken,
        CancellationToken cancellationToken)
    {
        var clientId = _configuration["Authentication:Google:ClientId"];

        if (string.IsNullOrWhiteSpace(clientId))
        {
            _logger.LogError("Google ClientIdが設定されていません");
            throw new InvalidOperationException("Google認証の設定が不正です");
        }

        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { clientId }
        };

        return await GoogleJsonWebSignature.ValidateAsync(
            idToken,
            validationSettings);
    }

    /// <summary>
    /// 新規ユーザーを作成
    /// </summary>
    private async Task<UserEntity> CreateNewUserAsync(
        GoogleJsonWebSignature.Payload payload,
        CancellationToken cancellationToken)
    {
        var userId = Guid.NewGuid();
        var tenantId = GetDefaultTenantId();

        // DisplayName の生成(null安全)
        var displayName = GetDisplayName(payload);

        // ユーザーエンティティ作成
        var user = new UserEntity
        {
            Id = userId,
            DisplayName = displayName,
            Email = payload.Email,
            PictureUrl = payload.Picture,
            TenantId = tenantId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        await _userWriteRepository.AddAsync(user, cancellationToken);

        // 外部ログイン情報作成
        var externalLogin = new UserExternalLoginEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ProviderName = AuthProvider.Google,
            ProviderKey = payload.Subject,
            TenantId = tenantId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        await _externalLoginRepository.CreateAsync(externalLogin, cancellationToken);

        return user;
    }

    /// <summary>
    /// 既存ユーザーのプロフィール情報を更新
    /// </summary>
    private async Task UpdateUserProfileAsync(
        UserEntity user,
        GoogleJsonWebSignature.Payload payload,
        CancellationToken cancellationToken)
    {
        var isUpdated = false;

        // 表示名の更新
        var newDisplayName = GetDisplayName(payload);
        if (user.DisplayName != newDisplayName)
        {
            user.DisplayName = newDisplayName;
            isUpdated = true;
        }

        // メールアドレスの更新
        if (!string.IsNullOrWhiteSpace(payload.Email) && user.Email != payload.Email)
        {
            user.Email = payload.Email;
            isUpdated = true;
        }

        // プロフィール画像の更新
        if (!string.IsNullOrWhiteSpace(payload.Picture) && user.PictureUrl != payload.Picture)
        {
            user.PictureUrl = payload.Picture;
            isUpdated = true;
        }

        if (isUpdated)
        {
            user.UpdatedAt = DateTimeOffset.UtcNow;
            user.UpdatedBy = user.Id;
            await _userWriteRepository.UpdateAsync(user, cancellationToken);

            _logger.LogDebug(
                "ユーザープロフィールを更新しました。UserId: {UserId}",
                user.Id);
        }
    }

    /// <summary>
    /// Googleペイロードから表示名を取得(null安全)
    /// </summary>
    private string GetDisplayName(GoogleJsonWebSignature.Payload payload)
    {
        // 優先順位: Name → Email → "ユーザー"
        if (!string.IsNullOrWhiteSpace(payload.Name))
            return payload.Name;

        if (!string.IsNullOrWhiteSpace(payload.Email))
            return payload.Email;

        // 最終フォールバック
        return "ユーザー";
    }

    /// <summary>
    /// デフォルトTenantIdを取得
    /// </summary>
    private Guid GetDefaultTenantId()
    {
        var tenantIdString = _configuration["DefaultTenantId"]
            ?? "deadeade-0001-0000-0000-000000000001";

        if (!Guid.TryParse(tenantIdString, out var tenantId))
        {
            _logger.LogWarning("デフォルトTenantIdの解析に失敗しました。ハードコード値を使用します。");
            return Guid.Parse("deadeade-0001-0000-0000-000000000001");
        }

        return tenantId;
    }

    #endregion
}
