namespace ServerlessKakeibo.Api.Service.Models;

/// <summary>
/// Vertex AIレスポンス
/// </summary>
public class VertexAiResponse
{
    /// <summary>
    /// 生成されたコンテンツ
    /// </summary>
    public string GeneratedContent { get; set; } = string.Empty;
}
