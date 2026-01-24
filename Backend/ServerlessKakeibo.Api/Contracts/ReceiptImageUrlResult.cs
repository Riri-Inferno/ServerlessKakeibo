namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// レシート画像URL結果
/// </summary>
public class ReceiptImageUrlResult
{
    /// <summary>
    /// 署名付きURL（1時間有効）
    /// </summary>
    public string SignedUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL有効期限
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
}
