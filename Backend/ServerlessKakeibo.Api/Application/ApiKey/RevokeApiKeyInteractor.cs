using ServerlessKakeibo.Api.Application.ApiKey.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.ApiKey;

/// <summary>
/// APIキー失効インタラクター
/// 物理削除はせず、RevokedAt を立てる
/// </summary>
public class RevokeApiKeyInteractor : IRevokeApiKeyUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IGenericReadRepository<ApiKeyEntity> _apiKeyReadRepository;
    private readonly IGenericWriteRepository<ApiKeyEntity> _apiKeyWriteRepository;
    private readonly ILogger<RevokeApiKeyInteractor> _logger;

    public RevokeApiKeyInteractor(
        ITransactionHelper transactionHelper,
        IGenericReadRepository<ApiKeyEntity> apiKeyReadRepository,
        IGenericWriteRepository<ApiKeyEntity> apiKeyWriteRepository,
        ILogger<RevokeApiKeyInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _apiKeyReadRepository = apiKeyReadRepository ?? throw new ArgumentNullException(nameof(apiKeyReadRepository));
        _apiKeyWriteRepository = apiKeyWriteRepository ?? throw new ArgumentNullException(nameof(apiKeyWriteRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(
        Guid userId,
        Guid apiKeyId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("ユーザーIDが無効です", nameof(userId));
        if (apiKeyId == Guid.Empty)
            throw new ArgumentException("APIキーIDが無効です", nameof(apiKeyId));

        await _transactionHelper.ExecuteInTransactionAsync(async () =>
        {
            var key = await _apiKeyReadRepository.GetByIdAsync(apiKeyId, cancellationToken);

            // 他ユーザーのキーを破棄できないようにする（404 と同義に扱う）
            if (key == null || key.UserId != userId)
                throw new KeyNotFoundException("APIキーが見つかりません");

            if (key.RevokedAt != null)
            {
                _logger.LogInformation(
                    "既に失効済みのAPIキーです。ApiKeyId: {ApiKeyId}", apiKeyId);
                return;
            }

            key.RevokedAt = DateTimeOffset.UtcNow;
            key.UpdatedBy = userId;
            await _apiKeyWriteRepository.UpdateAsync(key, cancellationToken);

            _logger.LogInformation(
                "APIキーを失効しました。UserId: {UserId}, ApiKeyId: {ApiKeyId}",
                userId, apiKeyId);
        });
    }
}
