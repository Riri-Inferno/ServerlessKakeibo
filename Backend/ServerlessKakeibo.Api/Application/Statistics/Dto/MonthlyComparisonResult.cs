using ServerlessKakeibo.Api.Application.TransactionSummary.Dto;

namespace ServerlessKakeibo.Api.Application.Statistics.Dto;

/// <summary>
/// 前月比込みの月次サマリー結果
/// </summary>
public class MonthlyComparisonResult
{
    /// <summary>
    /// 当月のサマリー
    /// </summary>
    public MonthlySummaryResult Current { get; set; } = null!;

    /// <summary>
    /// 前月のサマリー（データがない場合はnull）
    /// </summary>
    public MonthlySummaryResult? Previous { get; set; }

    /// <summary>
    /// 収入の前月比（%）
    /// </summary>
    public decimal? IncomeChangePercent { get; set; }

    /// <summary>
    /// 支出の前月比（%）
    /// </summary>
    public decimal? ExpenseChangePercent { get; set; }

    /// <summary>
    /// 収支の前月比（%）
    /// </summary>
    public decimal? BalanceChangePercent { get; set; }
}
