using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// ClaimsPrincipal拡張メソッド
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// ユーザーIDを取得
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)
                       ?? principal.FindFirst(JwtRegisteredClaimNames.Sub);

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("ユーザーIDを取得できませんでした");
        }

        return userId;
    }

    /// <summary>
    /// テナントIDを取得
    /// </summary>
    public static Guid GetTenantId(this ClaimsPrincipal principal)
    {
        var tenantIdClaim = principal.FindFirst("tenant_id");

        if (tenantIdClaim == null || !Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            throw new UnauthorizedAccessException("テナントIDを取得できませんでした");
        }

        return tenantId;
    }

    /// <summary>
    /// 表示名を取得
    /// </summary>
    public static string GetDisplayName(this ClaimsPrincipal principal)
    {
        var nameClaim = principal.FindFirst(ClaimTypes.Name)
                     ?? principal.FindFirst(JwtRegisteredClaimNames.Name);

        return nameClaim?.Value ?? "ユーザー";
    }

    /// <summary>
    /// メールアドレスを取得
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        var emailClaim = principal.FindFirst(ClaimTypes.Email)
                      ?? principal.FindFirst(JwtRegisteredClaimNames.Email);

        return emailClaim?.Value;
    }
}
