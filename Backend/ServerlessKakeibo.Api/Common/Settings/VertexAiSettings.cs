namespace ServerlessKakeibo.Api.Common.Settings;

/// <summary>
/// Vertex AI設定
/// </summary>
public class VertexAiSettings
{
   /// <summary>
   /// GCPプロジェクトID
   /// </summary>
   public string ProjectId { get; set; } = string.Empty;

   /// <summary>
   /// リージョン/ロケーション
   /// </summary>
   public string Location { get; set; } = "us-central1";

   /// <summary>
   /// デフォルトのモデルID
   /// </summary>
   public string DefaultModelId { get; set; } = "gemini-2.5-flash";

   /// <summary>
   /// 生成時の温度パラメータ（0.0-1.0）
   /// </summary>
   public double Temperature { get; set; } = 0.1;

   /// <summary>
   /// Top-P サンプリングパラメータ
   /// </summary>
   public double TopP { get; set; } = 0.40;

   /// <summary>
   /// Top-K サンプリングパラメータ
   /// </summary>
   public int TopK { get; set; } = 10;

   /// <summary>
   /// 最大出力トークン数
   /// </summary>
   public int MaxOutputTokens { get; set; } = 8192;

   /// <summary>
   /// 候補生成数
   /// </summary>
   public int CandidateCount { get; set; } = 1;

   /// <summary>
   /// APIリクエストのタイムアウト（秒）
   /// </summary>
   public int TimeoutSeconds { get; set; } = 60;

   /// <summary>
   /// APIエラー時の最大リトライ回数
   /// </summary>
   public int MaxRetries { get; set; } = 3;

   /// <summary>
   /// リトライ間隔（ミリ秒）
   /// </summary>
   public int RetryDelayMilliseconds { get; set; } = 1000;

   /// <summary>
   /// セーフティフィルタリングを有効にするか
   /// </summary>
   public bool EnableSafetyFiltering { get; set; } = true;
}
