using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;
using ServerlessKakeibo.Api.Service.Authentication.OAuth;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// OAuth の承認画面（フロントエンド）から呼ぶ API。
/// ログイン中のユーザー本人（JWT）だけが使える。API キーでは承認できない
/// </summary>
[ApiController]
[Route("api/oauth")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class OAuthConsentController : ControllerBase
{
    private readonly OAuthServerSettings _settings;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly ConsentTicketService _tickets;

    public OAuthConsentController(
        IOptions<OAuthServerSettings> settings,
        IOpenIddictApplicationManager applicationManager,
        ConsentTicketService tickets)
    {
        _settings = settings.Value;
        _applicationManager = applicationManager;
        _tickets = tickets;
    }

    /// <summary>
    /// 承認画面に表示するクライアントの情報
    /// </summary>
    [HttpGet("authorize-info")]
    [SwaggerOperation(
        Summary = "連携を求めているクライアントの情報を取得",
        Description = "承認画面に表示するクライアント名とコールバック先のホスト名を返す。client_id と redirect_uri は認可リクエストの値をそのまま渡す。")]
    [ProducesResponseType(typeof(ApiResponse<OAuthAuthorizeInfo>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OAuthAuthorizeInfo>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<OAuthAuthorizeInfo>>> GetAuthorizeInfoAsync(
        [FromQuery(Name = "client_id")] string clientId,
        [FromQuery(Name = "redirect_uri")] string redirectUri,
        [FromQuery(Name = "scope")] string? scope)
    {
        if (!_settings.IsEnabled)
        {
            return NotFound(ApiResponse<OAuthAuthorizeInfo>.Fail(ApiStatus.NotFound, "OAuth は無効です"));
        }

        var application = await FindValidClientAsync(clientId, redirectUri);
        if (application is null)
        {
            return BadRequest(ApiResponse<OAuthAuthorizeInfo>.Fail(
                ApiStatus.InvalidRequest, "クライアントまたはコールバック URL が正しくありません"));
        }

        var properties = await _applicationManager.GetPropertiesAsync(application);
        var clientUri = properties.TryGetValue("client_uri", out var value) ? value.GetString() : null;

        var requestedScopes = (scope ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var scopes = requestedScopes.Length == 0
            ? OAuthScopes.All.ToList()
            : requestedScopes.Where(OAuthScopes.All.Contains).ToList();

        return Ok(ApiResponse<OAuthAuthorizeInfo>.Success(new OAuthAuthorizeInfo(
            ClientId: clientId,
            ClientName: await _applicationManager.GetDisplayNameAsync(application) ?? new Uri(clientId).Host,
            ClientUri: clientUri,
            RedirectHost: new Uri(redirectUri).Host,
            RequestedScopes: scopes)));
    }

    /// <summary>
    /// 連携を許可し、承認チケットを発行する。
    /// フロントエンドは、認可リクエストのパラメータに consent_ticket を付けて /oauth/authorize に戻る
    /// </summary>
    [HttpPost("consent")]
    [SwaggerOperation(
        Summary = "連携を許可する",
        Description = "承認チケット（5 分で失効）を返す。scopes は read か read write（write だけは不可）。")]
    [ProducesResponseType(typeof(ApiResponse<OAuthConsentResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OAuthConsentResult>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<OAuthConsentResult>>> ConsentAsync(
        [FromBody] OAuthConsentRequest request)
    {
        if (!_settings.IsEnabled)
        {
            return NotFound(ApiResponse<OAuthConsentResult>.Fail(ApiStatus.NotFound, "OAuth は無効です"));
        }

        if (User.IsDelegatedAccess())
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<OAuthConsentResult>.Fail(ApiStatus.Forbidden, "API キーや連携中のアプリからは、連携を許可できません"));
        }

        var scopes = request.Scopes.Distinct().ToList();
        if (scopes.Count == 0 || !scopes.All(OAuthScopes.All.Contains) ||
            (scopes.Contains(OAuthScopes.Write) && !scopes.Contains(OAuthScopes.Read)))
        {
            return BadRequest(ApiResponse<OAuthConsentResult>.Fail(
                ApiStatus.InvalidRequest, "scopes は read、または read と write を指定してください"));
        }

        if (string.IsNullOrEmpty(request.CodeChallenge) ||
            await FindValidClientAsync(request.ClientId, request.RedirectUri) is null)
        {
            return BadRequest(ApiResponse<OAuthConsentResult>.Fail(
                ApiStatus.InvalidRequest, "クライアント、コールバック URL、または code_challenge が正しくありません"));
        }

        var ticket = _tickets.Issue(new ConsentTicket(
            User.GetUserId(),
            User.GetTenantId(),
            request.ClientId,
            request.RedirectUri,
            request.CodeChallenge,
            scopes));

        return Ok(ApiResponse<OAuthConsentResult>.Success(new OAuthConsentResult(ticket)));
    }

    private async Task<object?> FindValidClientAsync(string clientId, string redirectUri)
    {
        if (string.IsNullOrEmpty(clientId) || !Uri.TryCreate(redirectUri, UriKind.Absolute, out _))
        {
            return null;
        }

        var application = await _applicationManager.FindByClientIdAsync(clientId);
        return application is not null && await _applicationManager.ValidateRedirectUriAsync(application, redirectUri)
            ? application
            : null;
    }
}

public record OAuthAuthorizeInfo(
    string ClientId,
    string ClientName,
    string? ClientUri,
    string RedirectHost,
    IReadOnlyList<string> RequestedScopes);

public record OAuthConsentRequest(
    string ClientId,
    string RedirectUri,
    string CodeChallenge,
    IReadOnlyList<string> Scopes);

public record OAuthConsentResult(string ConsentTicket);
