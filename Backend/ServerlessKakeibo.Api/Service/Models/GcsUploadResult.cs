namespace ServerlessKakeibo.Api.Service.Models;

/// <summary>
/// GCSファイルアップロード結果
/// </summary>
public class GcsUploadResult
{
    /// <summary>
    /// バケット内のオブジェクトパス（DB保存用の一意識別子）
    /// </summary>
    /// <example>receipts/production/12345678-1234-1234-1234-123456789abc/202401/a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg</example>
    public string ObjectPath { get; set; } = string.Empty;

    /// <summary>
    /// 公開アクセス用URL（バケットが公開設定の場合のみ）
    /// </summary>
    /// <example>https://storage.googleapis.com/my-bucket/receipts/production/...</example>
    public string? PublicUrl { get; set; }

    /// <summary>
    /// 署名付き一時URL（プライベートバケット用）
    /// </summary>
    /// <example>https://storage.googleapis.com/my-bucket/receipts/...?X-Goog-Algorithm=...</example>
    public string? SignedUrl { get; set; }

    /// <summary>
    /// コンテンツタイプ
    /// </summary>
    /// <example>image/jpeg</example>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// ファイルサイズ（バイト）
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// アップロード完了日時（UTC）
    /// </summary>
    public DateTimeOffset UploadedAt { get; set; }

    /// <summary>
    /// カスタムメタデータ
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>(); // Dictionary → IDictionary に変更
}
