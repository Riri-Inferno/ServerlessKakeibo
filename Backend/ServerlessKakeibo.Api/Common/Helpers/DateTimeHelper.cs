using System.Globalization;

namespace ServerlessKakeibo.Api.Common.Helpers;

/// <summary>
/// 日時変換ヘルパー
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// 日付文字列を UTC の DateTimeOffset に変換
    /// </summary>
    /// <param name="dateString">日付文字列（例: "2026-01-27"）</param>
    /// <returns>UTC の DateTimeOffset</returns>
    /// <exception cref="ArgumentException">日付文字列が不正な場合</exception>
    public static DateTimeOffset ParseAsUtc(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            throw new ArgumentException("日付文字列が空です", nameof(dateString));

        // "2026-01-27" 形式（日付のみ）の場合、UTC の 00:00:00 として解釈
        if (DateTime.TryParseExact(
            dateString,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var dateOnly))
        {
            // Kind を明示的に Utc に設定
            var utcDate = DateTime.SpecifyKind(dateOnly, DateTimeKind.Utc);
            return new DateTimeOffset(utcDate);
        }

        // ISO 8601 形式（"2026-01-27T00:00:00Z" など）
        if (DateTimeOffset.TryParse(
            dateString,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out var dateTimeOffset))
        {
            return dateTimeOffset.ToUniversalTime();
        }

        throw new ArgumentException($"日付の解析に失敗しました: {dateString}", nameof(dateString));
    }

    /// <summary>
    /// DateTimeOffset を UTC の日付（00:00:00）に正規化
    /// </summary>
    /// <param name="value">変換元の DateTimeOffset</param>
    /// <returns>UTC の DateTimeOffset（時刻は 00:00:00）</returns>
    public static DateTimeOffset NormalizeToUtcDate(DateTimeOffset value)
    {
        return new DateTimeOffset(
            value.Year,
            value.Month,
            value.Day,
            0, 0, 0,
            TimeSpan.Zero
        );
    }
}
