namespace ServerlessKakeibo.Api.Common.Helpers;

/// <summary>
/// 締め日を考慮した期間計算ヘルパー
/// </summary>
public static class ClosingDayHelper
{
    /// <summary>
    /// 締め日を考慮した期間を取得
    /// </summary>
    /// <param name="year">対象年</param>
    /// <param name="month">対象月</param>
    /// <param name="closingDay">締め日(1-31, nullは月末締め)</param>
    /// <returns>期間の開始日と終了日</returns>
    public static (DateTimeOffset Start, DateTimeOffset End) GetPeriod(
        int year,
        int month,
        int? closingDay)
    {
        if (closingDay == null)
        {
            // 月末締め: 1日 00:00:00 〜 末日 23:59:59
            var start = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var end = new DateTimeOffset(year, month, daysInMonth, 23, 59, 59, 999, TimeSpan.Zero)
                .AddTicks(9999); // 23:59:59.9999999

            return (start, end);
        }
        else
        {
            // カスタム締め: 前月の締め日+1 〜 当月の締め日
            var prevMonth = month == 1 ? 12 : month - 1;
            var prevYear = month == 1 ? year - 1 : year;

            // 前月の日数を取得
            var daysInPrevMonth = DateTime.DaysInMonth(prevYear, prevMonth);
            var actualClosingDayPrev = Math.Min(closingDay.Value, daysInPrevMonth);

            // 当月の日数を取得
            var daysInCurrentMonth = DateTime.DaysInMonth(year, month);
            var actualClosingDayCurrent = Math.Min(closingDay.Value, daysInCurrentMonth);

            // 開始日: 前月の締め日 + 1日
            var start = new DateTimeOffset(prevYear, prevMonth, actualClosingDayPrev, 0, 0, 0, TimeSpan.Zero)
                .AddDays(1);

            // 終了日: 当月の締め日 23:59:59.9999999
            var end = new DateTimeOffset(year, month, actualClosingDayCurrent, 23, 59, 59, 999, TimeSpan.Zero)
                .AddTicks(9999);

            return (start, end);
        }
    }

    /// <summary>
    /// 月の範囲リストを生成(月次推移用)
    /// </summary>
    /// <param name="months">取得する月数</param>
    /// <param name="baseYear">基準年</param>
    /// <param name="baseMonth">基準月</param>
    /// <returns>年月のリスト(古い順)</returns>
    public static List<(int Year, int Month)> GenerateMonthlyRanges(
        int months,
        int baseYear,
        int baseMonth)
    {
        var ranges = new List<(int Year, int Month)>();
        var date = new DateTime(baseYear, baseMonth, 1);

        for (int i = months - 1; i >= 0; i--)
        {
            var targetDate = date.AddMonths(-i);
            ranges.Add((targetDate.Year, targetDate.Month));
        }

        return ranges;
    }
}
