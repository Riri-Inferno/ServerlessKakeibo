using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Service.Authentication.OAuth;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// OAuth の認可エンドポイント（/oauth/authorize）。
///
/// OpenIddict がリクエストを検証したあと（クライアント、コールバック URL、PKCE、resource、scope）にここへ渡す（passthrough）。
/// 1. 承認チケットがなければ、承認画面（フロントエンド）にリダイレクトする
/// 2. 承認画面でユーザーが許可すると、承認 API がチケットを発行し、フロントエンドがチケットを付けてここへ戻す
/// 3. チケットが正しければ、連携（OpenIddict の authorization）を記録して認可コードを発行する
/// </summary>
[ApiExplorerSettings(IgnoreApi = true)]
public class OAuthAuthorizationController : ControllerBase
{
    /// <summary>承認画面でユーザーが拒否したことを示すパラメータ</summary>
    public const string DeniedParameter = "consent_denied";

    private readonly OAuthServerSettings _settings;
    private readonly ConsentTicketService _tickets;
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly ApplicationDbContext _db;

    public OAuthAuthorizationController(
        IOptions<OAuthServerSettings> settings,
        ConsentTicketService tickets,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        ApplicationDbContext db)
    {
        _settings = settings.Value;
        _tickets = tickets;
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _db = db;
    }

    [HttpGet("~/" + OAuthServerServiceCollectionExtensions.AuthorizationEndpoint)]
    [HttpPost("~/" + OAuthServerServiceCollectionExtensions.AuthorizationEndpoint)]
    public async Task<IActionResult> AuthorizeAsync()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        if (request is null || !_settings.IsEnabled)
        {
            return NotFound();
        }

        if (request[DeniedParameter] is not null)
        {
            return Reject(Errors.AccessDenied, "ユーザーが連携を許可しませんでした。");
        }

        var token = (string?)request[ConsentTicketService.RequestParameter];
        if (string.IsNullOrEmpty(token))
        {
            return Redirect(BuildConsentPageUrl(request));
        }

        var ticket = await _tickets.ValidateAsync(token, request.ClientId, request.RedirectUri, request.CodeChallenge);
        if (ticket is null)
        {
            return Reject(Errors.AccessDenied, "承認の有効期限が切れたか、承認の内容が認可リクエストと一致しません。もう一度接続してください。");
        }

        // 承認したスコープは、クライアントが求めた範囲に限る（求めていなければ、承認した範囲をそのまま使う）
        var requestedScopes = request.GetScopes();
        if (!requestedScopes.IsEmpty && !ticket.Scopes.All(requestedScopes.Contains))
        {
            return Reject(Errors.InvalidScope, "承認したスコープが、クライアントの求めた範囲を超えています。");
        }

        var userExists = await _db.Users.AnyAsync(u => u.Id == ticket.UserId && !u.IsDeleted);
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId!);
        if (!userExists || application is null)
        {
            return Reject(Errors.AccessDenied, "ユーザーまたはクライアントが見つかりません。");
        }

        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, ticket.UserId.ToString())
                .SetClaim("tenant_id", ticket.TenantId.ToString());

        // offline_access は、OpenIddict がリフレッシュトークンを発行する条件。
        // MCP の仕様に合わせてメタデータでは公開せず、承認したときにサーバー側で付ける
        identity.SetScopes(ticket.Scopes.Append(Scopes.OfflineAccess));
        identity.SetResources(_settings.McpResource!);

        // 承認した連携を記録する。失効させると、この連携で発行したトークンもまとめて使えなくなる
        var authorization = await _authorizationManager.CreateAsync(
            identity: identity,
            subject: ticket.UserId.ToString(),
            client: (await _applicationManager.GetIdAsync(application))!,
            type: AuthorizationTypes.Permanent,
            scopes: identity.GetScopes());

        identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
        identity.SetDestinations(static _ => [Destinations.AccessToken]);

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    /// <summary>
    /// 認可リクエストのパラメータをそのまま付けて、承認画面の URL を作る
    /// </summary>
    private string BuildConsentPageUrl(OpenIddictRequest request)
    {
        var query = new List<KeyValuePair<string, string?>>();
        foreach (var (name, parameter) in request.GetParameters())
        {
            if (name is ConsentTicketService.RequestParameter or DeniedParameter)
            {
                continue;
            }

            var values = (StringValues)parameter;
            query.AddRange(values.Select(value => new KeyValuePair<string, string?>(name, value)));
        }

        return QueryHelpers.AddQueryString(_settings.ConsentPageUrl!, query);
    }

    private ForbidResult Reject(string error, string description) =>
        Forbid(
            new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description,
            }),
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
}
