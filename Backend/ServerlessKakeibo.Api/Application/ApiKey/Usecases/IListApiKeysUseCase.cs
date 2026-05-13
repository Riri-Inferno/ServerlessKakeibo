using ServerlessKakeibo.Api.Application.ApiKey.Dto;

namespace ServerlessKakeibo.Api.Application.ApiKey.Usecases;

/// <summary>
/// APIキー一覧取得ユースケース
/// </summary>
public interface IListApiKeysUseCase
{
    Task<IReadOnlyList<ApiKeyDto>> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
