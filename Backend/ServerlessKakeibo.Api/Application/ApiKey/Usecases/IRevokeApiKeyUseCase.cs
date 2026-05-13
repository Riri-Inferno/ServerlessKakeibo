namespace ServerlessKakeibo.Api.Application.ApiKey.Usecases;

/// <summary>
/// APIキー失効ユースケース
/// </summary>
public interface IRevokeApiKeyUseCase
{
    Task ExecuteAsync(
        Guid userId,
        Guid apiKeyId,
        CancellationToken cancellationToken = default);
}
