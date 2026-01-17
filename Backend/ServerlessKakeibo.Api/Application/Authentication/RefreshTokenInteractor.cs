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
    private readonly IGenericWriteRepository<UserEntity> _userWriteRepository;
    private readonly IJwtTokenService _jwtService;
    private readonly ILogger<RefreshTokenInteractor> _logger;

    public RefreshTokenInteractor(
        ITransactionHelper transactionHelper, 
        IGenericReadRepository<UserEntity> userReadRepository,
        IGenericWriteRepository<UserEntity> userWriteRepository,
        IJwtTokenService jwtService,
        ILogger<RefreshTokenInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper)); 
        _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        _userWriteRepository = userWriteRepository ?? throw new ArgumentNullException(nameof(userWriteRepository));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
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
                // 1. リフレッシュトークンからユーザーを検索（トランザクション内でロック）
                var user = await _userReadRepository.FirstOrDefaultAsync(
                    u => u.RefreshToken == refreshToken && !u.IsDeleted,
                    cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("無効なリフレッシュトークンです");
                    throw new UnauthorizedAccessException("リフレッシュトークンが無効です");
                }

                // 2. リフレッシュトークンの有効期限をチェック
                if (user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTimeOffset.UtcNow)
                {
                    _logger.LogWarning(
                        "リフレッシュトークンの有効期限が切れています。UserId: {UserId}",
                        user.Id);
                    throw new UnauthorizedAccessException("リフレッシュトークンの有効期限が切れています");
                }

                // 3. 新しいアクセストークンとリフレッシュトークンを生成
                var newAccessToken = _jwtService.GenerateToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                // 4. 新しいリフレッシュトークンをDBに保存（古いトークンを無効化）
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(_jwtService.RefreshTokenExpirationDays);
                user.UpdatedAt = DateTimeOffset.UtcNow;
                user.UpdatedBy = user.Id;
                await _userWriteRepository.UpdateAsync(user, cancellationToken);

                _logger.LogInformation(
                    "トークンを更新しました。UserId: {UserId}, NewRefreshTokenExpiry: {Expiry}",
                    user.Id, user.RefreshTokenExpiry);

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
