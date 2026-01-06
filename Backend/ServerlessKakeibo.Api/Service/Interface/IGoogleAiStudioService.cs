using ServerlessKakeibo.Api.Service.Models;

namespace ServerlessKakeibo.Api.Service.Interface;

/// <summary>
/// Google AI Studio (Gemini API) サービスインターフェース
/// </summary>
public interface IGoogleAiStudioService
{
    /// <summary>
    /// テキスト生成を実行
    /// </summary>
    /// <param name="userPrompt">ユーザープロンプト</param>
    /// <param name="systemPrompt">システムプロンプト（オプション）</param>
    /// <param name="images">画像のリスト（オプション）</param>
    /// <returns>生成されたテキスト</returns>
    Task<string> GenerateContentAsync(
        string userPrompt,
        string? systemPrompt = null,
        List<ImageAttachment>? images = null);
}
