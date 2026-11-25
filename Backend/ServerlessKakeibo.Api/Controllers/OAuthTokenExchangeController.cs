using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Validation;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Service.Authentication.OAuth;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// MCP サーバー向けのトークン交換（RFC 8693 の形式）。
///
/// MCP サーバーは、Claude などから受け取った OAuth のアクセストークン（宛先 = MCP サーバー）を
/// そのまま backend に渡してはならない（MCP の仕様で MUST NOT）。そこで、ここで検証して、
/// backend 宛ての短命なトークン（5 分）に交換する。
///
/// /internal/ 以下は Cloudflare Tunnel で外部から届かないようにしている（クラスタ内の MCP サーバーからだけ使う）。
/// 仮に外部から届いても、交換できるのは有効なアクセストークンを持っている場合だけで、
/// 得られる権限はそのトークンと同じ（ユーザー、スコープ）なので、権限が広がることはない。
/// </summary>
[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
[AllowAnonymous]
public class OAuthTokenExchangeController : ControllerBase
{
    public const string Route = "internal/oauth/token-exchange";

    private const string TokenExchangeGrantType = "urn:ietf:params:oauth:grant-type:token-exchange";
    private const string AccessTokenType = "urn:ietf:params:oauth:token-type:access_token";
    private const string JwtTokenType = "urn:ietf:params:oauth:token-type:jwt";

    private readonly IServiceProvider _services;
    private readonly IOptions<OAuthServerSettings> _settings;
    private readonly DelegatedTokenService _delegatedTokens;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<OAuthTokenExchangeController> _logger;

    public OAuthTokenExchangeController(
        IServiceProvider services,
        IOptions<OAuthServerSettings> settings,
        DelegatedTokenService delegatedTokens,
        ApplicationDbContext db,
        ILogger<OAuthTokenExchangeController> logger)
    {
        _services = services;
        _settings = settings;
        _delegatedTokens = delegatedTokens;
        _db = db;
        _logger = logger;
    }

    [HttpPost("~/" + Route)]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> ExchangeAsync(
        [FromForm(Name = "grant_type")] string? grantType,
        [FromForm(Name = "subject_token")] string? subjectToken,
        [FromForm(Name = "subject_token_type")] string? subjectTokenType,
        CancellationToken cancellationToken)
    {
        // OAuth が無効のときは、検証の仕組み自体が登録されていない
        var validation = _services.GetService<OpenIddictValidationService>();
        if (!_settings.Value.IsEnabled || validation is null)
        {
            return NotFound();
        }

        if (grantType != TokenExchangeGrantType ||
            subjectTokenType != AccessTokenType ||
            string.IsNullOrEmpty(subjectToken))
        {
            return Error(Errors.InvalidRequest, "grant_type、subject_token、subject_token_type を RFC 8693 の形式で指定してください。");
        }

        System.Security.Claims.ClaimsPrincipal principal;
        try
        {
            // 署名、期限、宛先（MCP サーバー）、トークンと連携が失効していないかを検証する
            principal = await validation.ValidateAccessTokenAsync(subjectToken, cancellationToken);
        }
        catch (OpenIddictExceptions.ProtocolException ex)
        {
            _logger.LogInformation("トークン交換を拒否しました: {Error} {Description}", ex.Error, ex.ErrorDescription);
            return Error(Errors.InvalidGrant, "アクセストークンが無効か、期限切れか、連携が解除されています。");
        }

        var authorizationId = principal.GetAuthorizationId();
        if (!Guid.TryParse(principal.GetClaim(Claims.Subject), out var userId) ||
            !Guid.TryParse(principal.GetClaim("tenant_id"), out var tenantId) ||
            string.IsNullOrEmpty(authorizationId))
        {
            return Error(Errors.InvalidGrant, "アクセストークンの内容が不足しています。");
        }

        var userExists = await _db.Users.AnyAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);
        if (!userExists)
        {
            return Error(Errors.InvalidGrant, "ユーザーが見つかりません。");
        }

        var scopes = principal.GetScopes().Where(OAuthScopes.All.Contains).ToList();
        var token = _delegatedTokens.Issue(
            userId, tenantId, authorizationId, principal.GetClaim(Claims.ClientId), scopes);

        return Ok(new Dictionary<string, object>
        {
            ["access_token"] = token,
            ["issued_token_type"] = JwtTokenType,
            ["token_type"] = "Bearer",
            ["expires_in"] = (int)OAuthDelegatedDefaults.Lifetime.TotalSeconds,
            ["scope"] = string.Join(' ', scopes),
        });
    }

    private BadRequestObjectResult Error(string error, string description) =>
        BadRequest(new Dictionary<string, string>
        {
            ["error"] = error,
            ["error_description"] = description,
        });
}
