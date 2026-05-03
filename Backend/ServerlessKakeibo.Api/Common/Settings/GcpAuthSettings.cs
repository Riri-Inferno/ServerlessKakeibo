namespace ServerlessKakeibo.Api.Common.Settings;

/// <summary>
/// GCP認証設定
/// </summary>
/// <remarks>
/// 認証は ADC (Application Default Credentials) に統一。
/// ローカル開発: <c>gcloud auth application-default login</c> または環境変数
/// <c>GOOGLE_APPLICATION_CREDENTIALS</c> を使用。
/// 本番 (k3s): WIF JSON をマウントし <c>GOOGLE_APPLICATION_CREDENTIALS</c> でパス指定。
/// </remarks>
public class GcpAuthSettings
{
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
