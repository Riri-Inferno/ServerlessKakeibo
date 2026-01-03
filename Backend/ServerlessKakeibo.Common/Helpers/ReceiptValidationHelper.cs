using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ServerlessKakeibo.Api.Common.Helpers;

/// <summary>
/// 領収書解析用のバリデーションヘルパークラス
/// </summary>
public static class ReceiptValidationHelper
{
    /// <summary>
    /// ファイルサイズの上限（デフォルト: 10MB）
    /// </summary>
    public const long DefaultMaxFileSize = 10 * 1024 * 1024;

    /// <summary>
    /// ファイルサイズが許容範囲内かチェック
    /// </summary>
    public static bool CheckFileSizeWithinLimit(IFormFile file, long maxSizeInBytes = DefaultMaxFileSize)
    {
        return file != null && file.Length > 0 && file.Length <= maxSizeInBytes;
    }

    /// <summary>
    /// ファイルが領収書解析用の画像として有効かチェック
    /// </summary>
    public static async Task<(bool IsValid, string? ErrorMessage)> CheckFileAsReceiptImageAsync(IFormFile file)
    {
        // ファイル存在チェック
        if (file == null)
            return (false, "ファイルが指定されていません");

        // ファイルサイズチェック
        if (!CheckFileSizeWithinLimit(file))
            return (false, $"ファイルサイズが上限（{DefaultMaxFileSize / 1024 / 1024}MB）を超えています");

        // 画像ファイルチェック
        if (!await ImageHelper.CheckIfImageFileAsync(file))
            return (false, "有効な画像ファイルではありません");

        return (true, null);
    }

    /// <summary>
    /// 日付文字列を解析して DateTimeOffset に変換
    /// </summary>
    public static DateTimeOffset? ParseDateString(string? dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        string[] formats =
        {
        // 時刻付きフォーマットを優先
        "yyyy-MM-dd HH:mm:ss",
        "yyyy/MM/dd HH:mm:ss",
        "yyyy年MM月dd日 HH時mm分ss秒",
        "yyyy年M月d日 H時m分s秒",
        "yyyy年MM月dd日 H:mm:ss",
        "yyyy-MM-dd HH:mm",
        "yyyy/MM/dd HH:mm",
        "HH:mm:ss yyyy-MM-dd",
        "HH:mm:ss yyyy/MM/dd",
        
        // 日付のみフォーマット
        "yyyy-MM-dd",
        "yyyy/MM/dd",
        "yyyy年MM月dd日",
        "yyyy年M月d日",
        "MM/dd/yyyy",
        "dd/MM/yyyy"
    };

        foreach (var format in formats)
        {
            if (DateTime.TryParseExact(dateString, format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
            {
                return new DateTimeOffset(date);
            }
        }

        // 一般的なパース試行
        if (DateTime.TryParse(dateString, out var parsedDate))
        {
            return new DateTimeOffset(parsedDate);
        }

        return null;
    }

    /// <summary>
    /// 金額文字列を decimal に変換
    /// </summary>
    public static decimal? ParseAmountString(string? amountString)
    {
        if (string.IsNullOrWhiteSpace(amountString))
            return null;

        // 円記号、カンマ、全角数字などを処理
        var cleaned = amountString
            .Replace("￥", "")
            .Replace("¥", "")
            .Replace("円", "")
            .Replace(",", "")
            .Replace("，", "")
            .Replace(" ", "")
            .Replace("　", "");

        // 全角数字を半角に変換
        cleaned = Regex.Replace(cleaned, "[０-９]", m =>
        {
            var fullWidthChar = m.Value[0];
            var halfWidthChar = (char)(fullWidthChar - '０' + '0');
            return halfWidthChar.ToString();
        });

        if (decimal.TryParse(cleaned, out var amount))
        {
            return amount;
        }

        return null;
    }
}
