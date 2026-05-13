using ServerlessKakeibo.Api.Application.ApiKey.Dto;
using ServerlessKakeibo.Api.Application.ApiKey.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.ApiKey;

/// <summary>
/// APIキー一覧取得インタラクター
/// </summary>
public class ListApiKeysInteractor : IListApiKeysUseCase
{
    private readonly IGenericReadRepository<ApiKeyEntity> _apiKeyReadRepository;
    private readonly ILogger<ListApiKeysInteractor> _logger;

    public ListApiKeysInteractor(
        IGenericReadRepository<ApiKeyEntity> apiKeyReadRepository,
        ILogger<ListApiKeysInteractor> logger)
    {
        _apiKeyReadRepository = apiKeyReadRepository ?? throw new ArgumentNullException(nameof(apiKeyReadRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<ApiKeyDto>> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("ユーザーIDが無効です", nameof(userId));

        var keys = await _apiKeyReadRepository.FindAsync(
            ak => ak.UserId == userId,
            cancellationToken);

        _logger.LogDebug(
            "APIキー一覧を取得しました。UserId: {UserId}, Count: {Count}",
            userId, keys.Count);

        return keys
            .OrderByDescending(ak => ak.CreatedAt)
            .Select(ak => new ApiKeyDto(
                ak.Id,
                ak.Name,
                ak.KeyPrefix,
                ak.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries),
                ak.ExpiresAt,
                ak.LastUsedAt,
                ak.RevokedAt,
                ak.CreatedAt
            ))
            .ToList();
    }
}
