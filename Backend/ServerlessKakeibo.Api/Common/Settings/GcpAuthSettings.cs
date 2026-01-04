namespace ServerlessKakeibo.Api.Common.Settings;

/// <summary>
/// GCP認証設定
/// </summary>
public class GcpAuthSettings
{
    /// <summary>
    /// サービスアカウントキーのパス
    /// </summary>
    public string? ServiceAccountKeyPath { get; set; }

    /// <summary>
    /// デフォルトのスコープ
    /// </summary>
    public string[] DefaultScopes { get; set; } = new[]
    {
        "https://www.googleapis.com/auth/cloud-platform"
    };

    /// <summary>
    /// アクセストークンのキャッシュを使用するか
    /// </summary>
    public bool UseCache { get; set; } = true;

    /// <summary>
    /// キャッシュの有効期限（分）
    /// </summary>
    public int CacheDurationMinutes { get; set; } = 60;
}
