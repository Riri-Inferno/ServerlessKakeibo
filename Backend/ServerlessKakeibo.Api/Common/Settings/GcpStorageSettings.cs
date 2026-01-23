namespace ServerlessKakeibo.Api.Common.Settings;

/// <summary>
/// GCPストレージ設定
/// </summary>
public class GcpStorageSettings
{
    /// <summary>
    /// バケット名
    /// </summary>
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// サービスアカウントキーのパス
    /// </summary>
    public string ServiceAccountKeyPath { get; set; } = string.Empty;
}
