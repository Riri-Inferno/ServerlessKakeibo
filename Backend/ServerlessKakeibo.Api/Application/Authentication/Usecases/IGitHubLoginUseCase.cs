using ServerlessKakeibo.Api.Application.Authentication.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.Authentication.Usecases;

/// <summary>
/// GitHub認証ログインユースケース
/// </summary>
public interface IGitHubLoginUseCase
{
    /// <summary>
    /// GitHub認証でログイン
    /// </summary>
    Task<LoginResult> ExecuteAsync(GitHubLoginRequest request, CancellationToken cancellationToken = default);
}
