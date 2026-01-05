namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// 日付範囲の値オブジェクト
/// </summary>
public record DateRange
{
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }

    public DateRange(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date must be before or equal to end date");

        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// 月の範囲を生成
    /// </summary>
    public static DateRange ForMonth(int year, int month)
    {
        var start = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddMonths(1).AddTicks(-1);
        return new DateRange(start, end);
    }

    /// <summary>
    /// 年の範囲を生成
    /// </summary>
    public static DateRange ForYear(int year)
    {
        var start = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddYears(1).AddTicks(-1);
        return new DateRange(start, end);
    }

    /// <summary>
    /// 指定日付が範囲内かチェック
    /// </summary>
    public bool Contains(DateTimeOffset date)
    {
        return date >= StartDate && date <= EndDate;
    }

    /// <summary>
    /// 日数を取得
    /// </summary>
    public int GetDays()
    {
        return (EndDate - StartDate).Days + 1;
    }
}
