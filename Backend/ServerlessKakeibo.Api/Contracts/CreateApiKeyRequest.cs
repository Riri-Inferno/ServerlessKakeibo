namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// APIキー発行リクエスト
/// </summary>
public class CreateApiKeyRequest
{
    /// <summary>
    /// キーのラベル（必須）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// スコープ集合。MVP では "read" のみ許可
    /// </summary>
    public List<string> Scopes { get; set; } = new() { "read" };

    /// <summary>
    /// 有効期限。null なら無期限
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
