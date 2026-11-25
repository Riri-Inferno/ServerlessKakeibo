using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ServerlessKakeibo.Api.Service.Authentication.OAuth;

/// <summary>
/// OAuth の連携経由で API を呼ぶための認証方式（MCP サーバーがトークン交換で受け取るトークン）。
/// API キーと同じ扱いにする:
/// - 使えるのは、API キーで使えるエンドポイント（JwtOrApiKeySchemes）だけ。画面専用の API（API キーの発行、連携の承認など）は使えない
/// - read / write のスコープを判定する
/// </summary>
public static class OAuthDelegatedDefaults
{
    /// <summary>認証スキーム名。identity の AuthenticationType にも使う</summary>
    public const string AuthenticationScheme = "OAuth";

    /// <summary>トークンの宛先。ログイン用の JWT（Authentication:Jwt:Audience）と区別する</summary>
    public const string Audience = "selene-api-oauth";

    /// <summary>どの連携（OpenIddict の authorization）によるアクセスかを示すクレーム</summary>
    public const string AuthorizationIdClaim = "oauth_authorization_id";

    /// <summary>連携しているクライアントの client_id</summary>
    public const string ClientIdClaim = "oauth_client_id";

    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
}

/// <summary>
/// トークン交換で発行する、backend 宛ての短命なトークン（JWT）を作る。
/// 署名はログイン用の JWT と同じ鍵で、宛先（aud）だけを変える
/// </summary>
public class DelegatedTokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly string? _issuer;

    public DelegatedTokenService(IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Authentication:Jwt");
        var secretKey = jwt["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey が設定されていません");

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        _issuer = jwt["Issuer"];
    }

    public SymmetricSecurityKey SigningKey => _key;

    public string? Issuer => _issuer;

    public string Issue(Guid userId, Guid tenantId, string authorizationId, string? clientId, IEnumerable<string> scopes)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new("tenant_id", tenantId.ToString()),
            new(OAuthDelegatedDefaults.AuthorizationIdClaim, authorizationId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        if (!string.IsNullOrEmpty(clientId))
        {
            claims.Add(new Claim(OAuthDelegatedDefaults.ClientIdClaim, clientId));
        }

        // API キーと同じく、スコープは 1 つずつ "scope" クレームにする（ApiKeyScopeHandler が見る）
        claims.AddRange(scopes.Select(scope => new Claim("scope", scope)));

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: OAuthDelegatedDefaults.Audience,
            claims: claims,
            expires: DateTime.UtcNow.Add(OAuthDelegatedDefaults.Lifetime),
            signingCredentials: new SigningCredentials(_key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
