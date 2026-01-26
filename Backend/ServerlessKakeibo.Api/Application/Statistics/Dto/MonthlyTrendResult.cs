namespace ServerlessKakeibo.Api.Application.Statistics.Dto;

/// <summary>
/// 月次推移結果
/// </summary>
public class MonthlyTrendResult
{
    /// <summary>
    /// 月ラベル（例: "2025年10月"）
    /// </summary>
    public List<MonthLabel> Months { get; set; } = new();

    /// <summary>
    /// 各月の収入
    /// </summary>
    public List<decimal> Incomes { get; set; } = new();

    /// <summary>
    /// 各月の支出
    /// </summary>
    public List<decimal> Expenses { get; set; } = new();

    /// <summary>
    /// 各月の収支
    /// </summary>
    public List<decimal> Balances { get; set; } = new();
}

/// <summary>
/// 月ラベル
/// </summary>
public class MonthLabel
{
    /// <summary>
    /// 年
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// 月
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// 表示用ラベル（例: "2025年10月"）
    /// </summary>
    public string Label { get; set; } = string.Empty;
}
