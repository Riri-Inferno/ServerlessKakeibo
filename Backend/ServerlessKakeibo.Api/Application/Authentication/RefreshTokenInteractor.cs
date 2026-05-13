using ServerlessKakeibo.Api.Application.Authentication.Dto;
using ServerlessKakeibo.Api.Application.Authentication.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Service.Interface;

namespace ServerlessKakeibo.Api.Application.Authentication;

/// <summary>
/// トークン更新インタラクター
/// </summary>
public class RefreshTokenInteractor : IRefreshTokenUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IGenericReadRepository<UserEntity> _userReadRepository;
    private readonly IGenericReadRepository<RefreshTokenEntity> _refreshTokenReadRepository;
    private readonly IGenericWriteRepository<RefreshTokenEntity> _refreshTokenWriteRepository;
    private readonly IJwtTokenService _jwtService;
    private readonly IPasswordHashService _passwordHashService;
    private readonly ILogger<RefreshTokenInteractor> _logger;

    public RefreshTokenInteractor(
        ITransactionHelper transactionHelper,
        IGenericReadRepository<UserEntity> userReadRepository,
        IGenericReadRepository<RefreshTokenEntity> refreshTokenReadRepository,
        IGenericWriteRepository<RefreshTokenEntity> refreshTokenWriteRepository,
        IJwtTokenService jwtService,
        IPasswordHashService passwordHashService,
        ILogger<RefreshTokenInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        _refreshTokenReadRepository = refreshTokenReadRepository ?? throw new ArgumentNullException(nameof(refreshTokenReadRepository));
        _refreshTokenWriteRepository = refreshTokenWriteRepository ?? throw new ArgumentNullException(nameof(refreshTokenWriteRepository));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _passwordHashService = passwordHashService ?? throw new ArgumentNullException(nameof(passwordHashService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

   /// <summary>
    /// リフレッシュトークンを使って新しいアクセストークンを取得
    /// </summary>
    public async Task<LoginResult> ExecuteAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("リフレッシュトークンが指定されていません", nameof(refreshToken));

        try
        {
            _logger.LogInformation("トークン更新処理を開始します");

            // トランザクション内で実行
            return await _transactionHelper.ExecuteInTransactionAsync(async () =>
            {
                // 1. 有効期限内のリフレッシュトークンを検索
                //    ハッシュ照合は受信値とDB側ハッシュを1件ずつ比較する必要があるため、候補を取得してループ
                var candidates = await _refreshTokenReadRepository.FindAsync(
                    rt => rt.ExpiresAt > DateTimeOffset.UtcNow,
                    cancellationToken);

                RefreshTokenEntity? matched = null;
                foreach (var candidate in candidates)
                {
                    if (_passwordHashService.VerifyPassword(refreshToken, candidate.TokenHash))
                    {
                        matched = candidate;
                        break;
                    }
                }

                if (matched == null)
                {
                    _logger.LogWarning("無効なリフレッシュトークンです");
                    throw new UnauthorizedAccessException("リフレッシュトークンが無効です");
                }

                // 2. ユーザーを取得（新しいアクセストークンの発行に必要）
                var user = await _userReadRepository.GetByIdAsync(matched.UserId, cancellationToken);
                if (user == null)
                {
                    _logger.LogWarning("リフレッシュトークンに対応するユーザーが見つかりません。UserId: {UserId}", matched.UserId);
                    throw new UnauthorizedAccessException("リフレッシュトークンが無効です");
                }

                // 3. 新しいアクセストークンとリフレッシュトークンを生成
                var newAccessToken = _jwtService.GenerateToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // 4. 旧レコードを論理削除し、新レコードを挿入（ローテーション）
                //    端末ごとにレコードが独立しているため、他端末のトークンは影響を受けない
                await _refreshTokenWriteRepository.SoftDeleteAsync(matched.Id, user.Id, cancellationToken);

                var newRefreshTokenEntity = new RefreshTokenEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TokenHash = _passwordHashService.HashPassword(newRefreshToken),
                    ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtService.RefreshTokenExpirationDays),
                    TenantId = user.TenantId,
                    CreatedBy = user.Id,
                    UpdatedBy = user.Id,
                };
                await _refreshTokenWriteRepository.AddAsync(newRefreshTokenEntity, cancellationToken);

                _logger.LogInformation(
                    "トークンを更新しました。UserId: {UserId}, NewRefreshTokenExpiry: {Expiry}",
                    user.Id, newRefreshTokenEntity.ExpiresAt);

                return new LoginResult(
                    newAccessToken,
                    newRefreshToken,
                    user.Id,
                    user.DisplayName,
                    user.PictureUrl
                );
            });
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "トークン更新中にエラーが発生しました");
            throw;
        }
    }
}
