using ServerlessKakeibo.Api.Application.Authentication.Dto;
using ServerlessKakeibo.Api.Application.Authentication.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Service.Interface;
using ServerlessKakeibo.Api.Service.Models;

namespace ServerlessKakeibo.Api.Application.Authentication;

/// <summary>
/// GitHub認証ログインインタラクター
/// </summary>
public class GitHubLoginInteractor : IGitHubLoginUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IGenericReadRepository<UserEntity> _userReadRepository;
    private readonly IGenericWriteRepository<UserEntity> _userWriteRepository;
    private readonly IUserExternalLoginRepository _externalLoginRepository;
    private readonly IGitHubAuthService _gitHubAuthService;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IGenericWriteRepository<UserSettingsEntity> _userSettingsWriteRepository;
    private readonly ICategoryInitializationService _categoryInitService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GitHubLoginInteractor> _logger;

    public GitHubLoginInteractor(
        ITransactionHelper transactionHelper,
        IGenericReadRepository<UserEntity> userReadRepository,
        IGenericWriteRepository<UserEntity> userWriteRepository,
        IUserExternalLoginRepository externalLoginRepository,
        IGitHubAuthService gitHubAuthService,
        IJwtTokenService jwtService,
        IPasswordHashService passwordHashService,
        IGenericWriteRepository<UserSettingsEntity> userSettingsWriteRepository,
        ICategoryInitializationService categoryInitService,
        IConfiguration configuration,
        ILogger<GitHubLoginInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        _userWriteRepository = userWriteRepository ?? throw new ArgumentNullException(nameof(userWriteRepository));
        _externalLoginRepository = externalLoginRepository ?? throw new ArgumentNullException(nameof(externalLoginRepository));
        _gitHubAuthService = gitHubAuthService ?? throw new ArgumentNullException(nameof(gitHubAuthService));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _passwordHashService = passwordHashService ?? throw new ArgumentNullException(nameof(passwordHashService));
        _userSettingsWriteRepository = userSettingsWriteRepository ?? throw new ArgumentNullException(nameof(userSettingsWriteRepository));
        _categoryInitService = categoryInitService ?? throw new ArgumentNullException(nameof(categoryInitService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// GitHub認証でログイン
    /// </summary>
    public async Task<LoginResult> ExecuteAsync(
        GitHubLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Code))
            throw new ArgumentException("認証コードが指定されていません", nameof(request));

        try
        {
            _logger.LogInformation("GitHub認証ログイン処理を開始します");

            return await _transactionHelper.ExecuteInTransactionAsync(async () =>
            {
                // 1. GitHub からユーザー情報を取得
                var gitHubUser = await _gitHubAuthService.GetUserInfoAsync(request.Code, cancellationToken);

                _logger.LogDebug(
                    "GitHubユーザー情報を取得しました。Login: {Login}, Email: {Email}",
                    gitHubUser.Login, gitHubUser.Email);

                // 2. 既存ユーザーを確認
                var externalLogin = await _externalLoginRepository
                    .GetByProviderAsync(AuthProvider.GitHub, gitHubUser.Id, cancellationToken);

                UserEntity user;

                if (externalLogin == null)
                {
                    // 3. 新規ユーザー作成
                    user = await CreateNewUserAsync(gitHubUser, cancellationToken);

                    _logger.LogInformation(
                        "新規GitHubユーザーを作成しました。UserId: {UserId}, Email: {Email}",
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
                    await UpdateUserProfileAsync(user, gitHubUser, cancellationToken);

                    _logger.LogInformation(
                        "既存GitHubユーザーでログインしました。UserId: {UserId}, Email: {Email}",
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
                    refreshToken,
                    user.Id,
                    user.DisplayName,
                    user.PictureUrl
                );
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub認証ログイン中にエラーが発生しました");
            throw;
        }
    }

    #region Private Methods

    /// <summary>
    /// 新規ユーザーを作成
    /// TODO: オーケストレーターとして共通化を検討
    /// </summary>
    private async Task<UserEntity> CreateNewUserAsync(
        GitHubUserInfo gitHubUser,
        CancellationToken cancellationToken)
    {
        var userId = Guid.NewGuid();
        var tenantId = GetDefaultTenantId();

        // DisplayName の生成
        var displayName = GetDisplayName(gitHubUser);

        // ユーザーエンティティ作成
        var user = new UserEntity
        {
            Id = userId,
            DisplayName = displayName,
            Email = gitHubUser.Email,
            PictureUrl = gitHubUser.AvatarUrl,
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
            ProviderName = AuthProvider.GitHub,
            ProviderKey = gitHubUser.Id,
            TenantId = tenantId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        await _externalLoginRepository.CreateAsync(externalLogin, cancellationToken);

        // UserSettings 作成
        var userSettings = new UserSettingsEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TimeZone = "Asia/Tokyo",
            CurrencyCode = "JPY",
            TenantId = tenantId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        await _userSettingsWriteRepository.AddAsync(userSettings, cancellationToken);
        await _userSettingsWriteRepository.SaveChangesAsync(cancellationToken);

        // カテゴリ初期化
        await _categoryInitService.InitializeUserCategoriesAsync(
            userSettings.Id,
            userId,
            tenantId,
            cancellationToken);

        return user;
    }

    /// <summary>
    /// 既存ユーザーのプロフィール情報を更新
    /// </summary>
    private async Task UpdateUserProfileAsync(
        UserEntity user,
        GitHubUserInfo gitHubUser,
        CancellationToken cancellationToken)
    {
        var isUpdated = false;

        // 表示名の更新
        var newDisplayName = GetDisplayName(gitHubUser);
        if (user.DisplayName != newDisplayName)
        {
            user.DisplayName = newDisplayName;
            isUpdated = true;
        }

        // メールアドレスの更新
        if (!string.IsNullOrWhiteSpace(gitHubUser.Email) && user.Email != gitHubUser.Email)
        {
            user.Email = gitHubUser.Email;
            isUpdated = true;
        }

        // プロフィール画像の更新
        if (!string.IsNullOrWhiteSpace(gitHubUser.AvatarUrl) && user.PictureUrl != gitHubUser.AvatarUrl)
        {
            user.PictureUrl = gitHubUser.AvatarUrl;
            isUpdated = true;
        }

        if (isUpdated)
        {
            user.UpdatedAt = DateTimeOffset.UtcNow;
            user.UpdatedBy = user.Id;
            await _userWriteRepository.UpdateAsync(user, cancellationToken);

            _logger.LogDebug(
                "GitHubユーザープロフィールを更新しました。UserId: {UserId}",
                user.Id);
        }
    }

    /// <summary>
    /// GitHubユーザー情報から表示名を取得
    /// </summary>
    private string GetDisplayName(GitHubUserInfo gitHubUser)
    {
        // 優先順位: Name → Login → Email → "ユーザー"
        if (!string.IsNullOrWhiteSpace(gitHubUser.Name))
            return gitHubUser.Name;

        if (!string.IsNullOrWhiteSpace(gitHubUser.Login))
            return gitHubUser.Login;

        if (!string.IsNullOrWhiteSpace(gitHubUser.Email))
            return gitHubUser.Email;

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
