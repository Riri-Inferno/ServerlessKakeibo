using ServerlessKakeibo.Api.Application.Authentication.Dto;

namespace ServerlessKakeibo.Api.Application.Authentication.Usecases;

public interface IGoogleLoginUseCase
{
    /// <summary>
    /// Googleログインを実行
    /// </summary>
    /// <param name="idToken"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<LoginResult> ExecuteAsync(
        string idToken,
        CancellationToken cancellationToken = default);
}
