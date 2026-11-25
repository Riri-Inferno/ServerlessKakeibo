using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ServerlessKakeibo.Api.Service.Authentication.OAuth;

/// <summary>
/// 承認チケット。承認画面（フロントエンド）でユーザーが許可したことを、認可エンドポイントに伝えるための短命なトークン。
///
/// SELENE のログイン状態はフロントエンドが持つ JWT で、ブラウザが認可エンドポイントに来るときには付いてこない。
/// そこで、承認 API（JWT で認証）がこのチケットを発行し、フロントエンドが認可エンドポイントに付けて戻す。
/// チケットは、client_id・redirect_uri・code_challenge に紐づけるので、別の認可リクエストには使えない。
/// </summary>
public class ConsentTicketService
{
    public const string RequestParameter = "consent_ticket";

    private const string Audience = "selene-oauth-consent";
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);

    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly JsonWebTokenHandler _handler = new();

    public ConsentTicketService(IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Authentication:Jwt");
        var secretKey = jwt["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey が設定されていません");

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        _issuer = jwt["Issuer"] ?? "selene";
    }

    public string Issue(ConsentTicket ticket)
    {
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = ticket.UserId.ToString(),
            ["tenant_id"] = ticket.TenantId.ToString(),
            ["client_id"] = ticket.ClientId,
            ["redirect_uri"] = ticket.RedirectUri,
            ["code_challenge"] = ticket.CodeChallenge,
            ["scope"] = string.Join(' ', ticket.Scopes),
        };

        return _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _issuer,
            Audience = Audience,
            Claims = claims,
            Expires = DateTime.UtcNow.Add(Lifetime),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256),
        });
    }

    /// <summary>
    /// チケットを検証する。署名・期限・宛先が正しく、認可リクエストの内容と一致する場合だけ返す
    /// </summary>
    public async Task<ConsentTicket?> ValidateAsync(
        string token, string? clientId, string? redirectUri, string? codeChallenge)
    {
        var result = await _handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = _issuer,
            ValidAudience = Audience,
            IssuerSigningKey = _key,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        });

        if (!result.IsValid)
        {
            return null;
        }

        var identity = result.ClaimsIdentity;
        string? Get(string type) => identity.FindFirst(type)?.Value;

        if (!Guid.TryParse(Get(ClaimTypes.NameIdentifier) ?? Get(JwtRegisteredClaimNames.Sub), out var userId) ||
            !Guid.TryParse(Get("tenant_id"), out var tenantId))
        {
            return null;
        }

        var ticket = new ConsentTicket(
            userId,
            tenantId,
            Get("client_id") ?? string.Empty,
            Get("redirect_uri") ?? string.Empty,
            Get("code_challenge") ?? string.Empty,
            (Get("scope") ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return string.Equals(ticket.ClientId, clientId, StringComparison.Ordinal) &&
               string.Equals(ticket.RedirectUri, redirectUri, StringComparison.Ordinal) &&
               string.Equals(ticket.CodeChallenge, codeChallenge, StringComparison.Ordinal)
            ? ticket
            : null;
    }
}

public record ConsentTicket(
    Guid UserId,
    Guid TenantId,
    string ClientId,
    string RedirectUri,
    string CodeChallenge,
    IReadOnlyList<string> Scopes);
