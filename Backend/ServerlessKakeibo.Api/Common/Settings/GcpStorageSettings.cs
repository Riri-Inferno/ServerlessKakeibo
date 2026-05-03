namespace ServerlessKakeibo.Api.Common.Settings;

/// <summary>
/// GCS設定
/// </summary>
public class GcpStorageSettings
{
    /// <summary>
    /// バケット名
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// GCPプロジェクトID
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>
    /// バケットが公開設定かどうか
    /// </summary>
    public bool IsPublicBucket { get; set; } = false;

    /// <summary>
    /// 署名付きURLを自動生成するか
    /// </summary>
    public bool GenerateSignedUrl { get; set; } = true;

    /// <summary>
    /// 署名付きURLの有効期限（時間）
    /// </summary>
    public int SignedUrlExpirationHours { get; set; } = 1;

    /// <summary>
    /// 署名用 SA email。IAM <c>signBlob</c> API 経由で署名するため、
    /// 認証主体（user / WIF）は本SAに対して
    /// <c>roles/iam.serviceAccountTokenCreator</c> を保持する必要あり。
    /// </summary>
    public string SignerServiceAccount { get; set; } = string.Empty;
}
