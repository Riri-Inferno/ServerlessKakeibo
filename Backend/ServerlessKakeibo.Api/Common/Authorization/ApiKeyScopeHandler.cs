using Microsoft.AspNetCore.Authorization;
using ServerlessKakeibo.Api.Service.Authentication.ApiKey;

namespace ServerlessKakeibo.Api.Common.Authorization;

/// <summary>
/// ApiKeyScopeRequirement のハンドラ
///
/// 認証スキームが ApiKey の場合のみスコープを要求する。
/// JWT で来たリクエストは素通りさせる（人間ユーザーは JWT で全機能を使える前提）。
/// </summary>
public class ApiKeyScopeHandler : AuthorizationHandler<ApiKeyScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ApiKeyScopeRequirement requirement)
    {
        var identity = context.User.Identities
            .FirstOrDefault(i => i.IsAuthenticated);

        if (identity == null)
        {
            // 未認証はそもそも認可ポリシーに到達しないが、念のため
            return Task.CompletedTask;
        }

        // ApiKey スキーム以外（JWT 等）は素通り
        if (!string.Equals(
                identity.AuthenticationType,
                ApiKeyAuthenticationDefaults.AuthenticationScheme,
                StringComparison.Ordinal))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        // ApiKey スキームの場合は scope クレームで判定
        var hasScope = identity.Claims.Any(c =>
            c.Type == "scope" &&
            string.Equals(c.Value, requirement.RequiredScope, StringComparison.Ordinal));

        if (hasScope)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
