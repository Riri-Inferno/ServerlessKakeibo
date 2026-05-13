using Microsoft.AspNetCore.Authorization;

namespace ServerlessKakeibo.Api.Common.Authorization;

/// <summary>
/// APIキー認証時に要求されるスコープ
/// JWT 認証で来たリクエストには適用しない（素通り）
/// </summary>
public class ApiKeyScopeRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// 要求するスコープ（例: "read", "write"）
    /// </summary>
    public string RequiredScope { get; }

    public ApiKeyScopeRequirement(string requiredScope)
    {
        if (string.IsNullOrWhiteSpace(requiredScope))
            throw new ArgumentException("RequiredScope は空にできません", nameof(requiredScope));

        RequiredScope = requiredScope;
    }
}
