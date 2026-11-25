using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;
using ServerlessKakeibo.Api.Service.Authentication.OAuth;
using Swashbuckle.AspNetCore.Annotations;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// OAuth で連携中のアプリ（Claude など）の一覧と、連携の解除。
/// ログイン中のユーザー本人（JWT）だけが使える。API キーや OAuth の連携経由では使えない
/// </summary>
[ApiController]
[Route("api/oauth/connections")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class OAuthConnectionController : ControllerBase
{
    private readonly IServiceProvider _services;

    public OAuthConnectionController(IServiceProvider services)
    {
        _services = services;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "連携中のアプリの一覧",
        Description = "OAuth で連携を許可したアプリの一覧。解除したものは含まない。")]
    [ProducesResponseType(typeof(ApiResponse<List<OAuthConnectionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<OAuthConnectionDto>>>> ListAsync(CancellationToken cancellationToken)
    {
        // OAuth が無効の環境でも画面が壊れないように、空の一覧を返す
        if (!TryGetManagers(out var authorizations, out var applications, out var tokens))
        {
            return Ok(ApiResponse<List<OAuthConnectionDto>>.Success([]));
        }

        var subject = User.GetUserId().ToString();
        var result = new List<OAuthConnectionDto>();

        await foreach (var authorization in authorizations.FindBySubjectAsync(subject, cancellationToken))
        {
            if (!await authorizations.HasStatusAsync(authorization, Statuses.Valid, cancellationToken))
            {
                continue;
            }

            var applicationId = await authorizations.GetApplicationIdAsync(authorization, cancellationToken);
            var application = applicationId is null ? null : await applications.FindByIdAsync(applicationId, cancellationToken);
            var id = await authorizations.GetIdAsync(authorization, cancellationToken);

            // 最後に使った日時の目安として、この連携で最後にトークンを発行した日時を使う
            // （アクセストークンは 60 分ごとに更新されるので、使っていればその頃の日時になる）
            DateTimeOffset? lastUsedAt = null;
            await foreach (var token in tokens.FindByAuthorizationIdAsync(id!, cancellationToken))
            {
                var created = await tokens.GetCreationDateAsync(token, cancellationToken);
                if (created > lastUsedAt || lastUsedAt is null)
                {
                    lastUsedAt = created;
                }
            }

            result.Add(new OAuthConnectionDto(
                Id: id!,
                ClientId: application is null ? null : await applications.GetClientIdAsync(application, cancellationToken),
                ClientName: application is null ? "（不明なアプリ）" : await applications.GetDisplayNameAsync(application, cancellationToken) ?? "（不明なアプリ）",
                Scopes: (await authorizations.GetScopesAsync(authorization, cancellationToken))
                    .Where(OAuthScopes.All.Contains).ToList(),
                CreatedAt: await authorizations.GetCreationDateAsync(authorization, cancellationToken),
                LastUsedAt: lastUsedAt));
        }

        return Ok(ApiResponse<List<OAuthConnectionDto>>.Success(
            result.OrderByDescending(c => c.CreatedAt).ToList()));
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "連携を解除する",
        Description = "連携を解除し、その連携で発行したトークンもすべて使えなくする。アプリから使うには、もう一度接続して許可する必要がある。")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> RevokeAsync(
        [FromRoute] string id, CancellationToken cancellationToken)
    {
        if (!TryGetManagers(out var authorizations, out _, out var tokens))
        {
            return NotFound(ApiResponse<object>.Fail(ApiStatus.NotFound, "連携が見つかりません"));
        }

        var authorization = await authorizations.FindByIdAsync(id, cancellationToken);
        // 他人の連携は「見つからない」として扱う
        if (authorization is null ||
            await authorizations.GetSubjectAsync(authorization, cancellationToken) != User.GetUserId().ToString())
        {
            return NotFound(ApiResponse<object>.Fail(ApiStatus.NotFound, "連携が見つかりません"));
        }

        await authorizations.TryRevokeAsync(authorization, cancellationToken);
        await tokens.RevokeByAuthorizationIdAsync(id, cancellationToken);

        return Ok(ApiResponse<object>.Success(new { id }));
    }

    private bool TryGetManagers(
        out IOpenIddictAuthorizationManager authorizations,
        out IOpenIddictApplicationManager applications,
        out IOpenIddictTokenManager tokens)
    {
        authorizations = _services.GetService<IOpenIddictAuthorizationManager>()!;
        applications = _services.GetService<IOpenIddictApplicationManager>()!;
        tokens = _services.GetService<IOpenIddictTokenManager>()!;
        return authorizations is not null && applications is not null && tokens is not null;
    }
}

public record OAuthConnectionDto(
    string Id,
    string? ClientId,
    string ClientName,
    IReadOnlyList<string> Scopes,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? LastUsedAt);
