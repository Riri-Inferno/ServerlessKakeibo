using Google.Apis.Auth.OAuth2;

namespace ServerlessKakeibo.Api.Service.Interface;

/// <summary>
/// GCP認証サービスのインターフェース
/// </summary>
public interface IGcpAuthService
{
    /// <summary>
    /// 指定されたスコープでGoogleCredentialを取得
    /// </summary>
    /// <param name="scopes">要求するスコープ</param>
    /// <returns>GoogleCredential</returns>
    Task<GoogleCredential> GetCredentialAsync(params string[] scopes);

    /// <summary>
    /// 指定されたスコープでアクセストークンを取得
    /// </summary>
    /// <param name="scopes">要求するスコープ</param>
    /// <returns>アクセストークン</returns>
    Task<string> GetAccessTokenAsync(params string[] scopes);
}
