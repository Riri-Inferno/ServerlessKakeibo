using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;

namespace ServerlessKakeibo.Api.Service.Authentication.ApiKey;

/// <summary>
/// APIキーでのリクエストが 401 / 403 になったとき、理由をレスポンス本文に書き込む。
/// MCP など機械のクライアントが、次に取るべき行動を判断できるようにするため。
/// JWT（ブラウザ）のリクエストには何もしない。
/// </summary>
public static class ApiKeyErrorResponder
{
    private const string WrittenFlag = "ApiKeyErrorResponder.Written";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Authorization ヘッダが APIキー（slk_ で始まる Bearer トークン）か
    /// </summary>
    public static bool IsApiKeyRequest(HttpContext context)
    {
        var raw = context.Request.Headers.Authorization.ToString();
        return AuthenticationHeaderValue.TryParse(raw, out var header)
            && string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
            && header.Parameter?.StartsWith(ApiKeyAuthenticationDefaults.KeyPrefix, StringComparison.Ordinal) == true;
    }

    /// <summary>
    /// 401 の本文を書き込む。APIキーのリクエストでなければ false を返して何もしない。
    /// 複数スキームの challenge から呼ばれても、書き込みは1回だけ行う。
    /// </summary>
    public static async Task<bool> TryWriteUnauthorizedAsync(HttpContext context)
    {
        if (!IsApiKeyRequest(context)) return false;
        if (context.Items.ContainsKey(WrittenFlag)) return true;

        // 認証結果はリクエスト内でキャッシュされるので、DB の照合は再実行されない
        var result = await context.AuthenticateAsync(ApiKeyAuthenticationDefaults.AuthenticationScheme);
        var message = result.Succeeded
            ? "このエンドポイントは API キーでは利用できません"
            : result.Failure?.Message ?? "APIキーが無効です";

        context.Response.Headers.WWWAuthenticate = "Bearer error=\"invalid_token\"";
        await WriteAsync(context, StatusCodes.Status401Unauthorized, ApiStatus.Unauthorized, message);
        return true;
    }

    /// <summary>
    /// 403 の本文を書き込む。APIキーのリクエストでなければ false を返して何もしない。
    /// </summary>
    public static async Task<bool> TryWriteForbiddenAsync(HttpContext context)
    {
        if (!IsApiKeyRequest(context)) return false;
        if (context.Items.ContainsKey(WrittenFlag)) return true;

        await WriteAsync(
            context,
            StatusCodes.Status403Forbidden,
            ApiStatus.Forbidden,
            "API キーのスコープが不足しています（書き込み系の操作には write スコープが必要です）");
        return true;
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, ApiStatus status, string message)
    {
        context.Items[WrittenFlag] = true;
        if (context.Response.HasStarted) return;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(
            JsonSerializer.Serialize(ApiResponse<object>.Fail(status, message), JsonOptions));
    }
}
