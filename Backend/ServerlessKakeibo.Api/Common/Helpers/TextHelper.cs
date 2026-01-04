using System.Text.Json;
using System.Text.RegularExpressions;

namespace ServerlessKakeibo.Api.Common.Helpers;

/// <summary>
/// テキスト処理に関するヘルパークラス
/// </summary>
public static class TextHelper
{
    /// <summary>
    /// カスタムプロンプトをサニタイズして安全にする
    /// </summary>
    /// <param name="customPrompt"></param>
    /// <returns></returns>
    public static string SanitizeCustomPrompt(string? customPrompt)
    {
        if (string.IsNullOrWhiteSpace(customPrompt))
            return "この画像を解析してください。";

        // 危険なキーワードを除去
        var dangerousKeywords = new[]
        {
        "ignore", "forget", "system", "admin", "override",
        "無視", "忘れ", "システム", "管理者", "上書き"
    };

        foreach (var keyword in dangerousKeywords)
        {
            customPrompt = customPrompt.Replace(keyword, "", StringComparison.OrdinalIgnoreCase);
        }

        // 長さ制限
        if (customPrompt.Length > 1000)
        {
            customPrompt = customPrompt.Substring(0, 1000);
        }

        return customPrompt;
    }
}
