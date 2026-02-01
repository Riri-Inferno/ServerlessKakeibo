using ServerlessKakeibo.Api.Service.Models;

namespace ServerlessKakeibo.Api.Service.Interface;

/// <summary>
/// GitHub認証サービス
/// </summary>
public interface IGitHubAuthService
{
    /// <summary>
    /// 認証コードからユーザー情報を取得
    /// </summary>
    Task<GitHubUserInfo> GetUserInfoAsync(string code, CancellationToken cancellationToken = default);
}
