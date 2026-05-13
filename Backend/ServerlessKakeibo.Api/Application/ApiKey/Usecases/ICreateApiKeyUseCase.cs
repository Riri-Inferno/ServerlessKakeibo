using ServerlessKakeibo.Api.Application.ApiKey.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.ApiKey.Usecases;

/// <summary>
/// APIキー発行ユースケース
/// </summary>
public interface ICreateApiKeyUseCase
{
    Task<CreateApiKeyResult> ExecuteAsync(
        Guid userId,
        CreateApiKeyRequest request,
        CancellationToken cancellationToken = default);
}
