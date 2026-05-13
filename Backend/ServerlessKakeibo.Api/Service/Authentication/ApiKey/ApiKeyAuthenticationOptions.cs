using Microsoft.AspNetCore.Authentication;

namespace ServerlessKakeibo.Api.Service.Authentication.ApiKey;

/// <summary>
/// APIキー認証スキームのオプション
/// 現時点で設定値はないが、AuthenticationHandler の型引数として必要
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
}
