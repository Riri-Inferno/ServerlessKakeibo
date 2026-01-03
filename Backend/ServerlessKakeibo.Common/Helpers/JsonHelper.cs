using System.Text.Json;
using System.Text.RegularExpressions;

namespace ServerlessKakeibo.Api.Common.Helpers;

/// <summary>
/// JSON処理に関するヘルパークラス
/// </summary>
public static class JsonHelper
{
    /// <summary>
    /// LLMレスポンスからJSONを抽出して有効性をチェック
    /// </summary>
    public static (bool IsValid, string? CleanedJson, string? ErrorMessage) ExtractAndValidateLlmJson(string llmResponse)
    {
        if (string.IsNullOrWhiteSpace(llmResponse))
            return (false, null, "レスポンスが空です");

        try
        {
            // JSONを抽出（コードブロックや余分な文字を削除）
            var cleanedJson = ExtractJsonFromLlmResponse(llmResponse);

            // JSON検証
            using var doc = JsonDocument.Parse(cleanedJson);

            return (true, cleanedJson, null);
        }
        catch (JsonException ex)
        {
            // 修復を試みる
            var repairedJson = AttemptToRepairJson(llmResponse);
            if (repairedJson != null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(repairedJson);
                    return (true, repairedJson, null);
                }
                catch
                {
                    return (false, null, $"JSON解析エラー: {ex.Message}");
                }
            }

            return (false, null, $"JSON解析エラー: {ex.Message}");
        }
    }

    /// <summary>
    /// LLMレスポンスからJSON部分を抽出
    /// </summary>
    private static string ExtractJsonFromLlmResponse(string response)
    {
        // ```json``` ブロックを除去
        var codeBlockPattern = @"```(?:json)?\s*(.*?)\s*```";
        var codeBlockMatch = Regex.Match(response, codeBlockPattern, RegexOptions.Singleline);
        if (codeBlockMatch.Success)
        {
            response = codeBlockMatch.Groups[1].Value;
        }

        // 先頭・末尾の空白を削除
        response = response.Trim();

        // JSONオブジェクトまたは配列の開始・終了位置を検出
        var firstBrace = response.IndexOfAny(new[] { '{', '[' });
        var lastBrace = Math.Max(response.LastIndexOf('}'), response.LastIndexOf(']'));

        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            response = response.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return response;
    }

    /// <summary>
    /// 壊れたJSONの修復を試みる
    /// </summary>
    private static string? AttemptToRepairJson(string brokenJson)
    {
        try
        {
            var cleaned = ExtractJsonFromLlmResponse(brokenJson);

            // 末尾のカンマを削除
            cleaned = Regex.Replace(cleaned, @",\s*([}\]])", "$1");

            // 不完全な文字列を閉じる
            var openQuotes = cleaned.Count(c => c == '"') % 2;
            if (openQuotes == 1)
            {
                cleaned += "\"";
            }

            // 不完全なブラケット/ブレースを閉じる
            var openBraces = cleaned.Count(c => c == '{') - cleaned.Count(c => c == '}');
            var openBrackets = cleaned.Count(c => c == '[') - cleaned.Count(c => c == ']');

            for (int i = 0; i < openBraces; i++)
                cleaned += "}";
            for (int i = 0; i < openBrackets; i++)
                cleaned += "]";

            // シングルクォートをダブルクォートに変換（JSONはダブルクォートのみ）
            cleaned = Regex.Replace(cleaned, @"(?<![\\])'", "\"");

            return cleaned;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// JSONを整形して読みやすくする
    /// </summary>
    public static string? PrettifyJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }
        catch
        {
            return null;
        }
    }
}
