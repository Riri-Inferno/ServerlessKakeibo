namespace ServerlessKakeibo.Api.Application.TransactionSummary.Dto;

/// <summary>
/// 月次サマリー結果
/// </summary>
public class MonthlySummaryResult
{
    /// <summary>
    /// 対象年
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// 対象月（1-12）
    /// </summary>
    public int Month { get; set; }

    /// <summary>
    /// 収入合計
    /// </summary>
    public decimal Income { get; set; }

    /// <summary>
    /// 支出合計
    /// </summary>
    public decimal Expense { get; set; }

    /// <summary>
    /// 差引（収入 - 支出）
    /// </summary>
    /// <remarks>
    /// 正の値：黒字、負の値：赤字、0：収支ゼロ
    /// </remarks>
    public decimal Balance { get; set; }

    /// <summary>
    /// 取引件数（合計）
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// 収入件数
    /// </summary>
    public int IncomeCount { get; set; }

    /// <summary>
    /// 支出件数
    /// </summary>
    public int ExpenseCount { get; set; }

    /// <summary>
    /// 支出トップ3カテゴリ
    /// </summary>
    public List<CategorySummary> TopExpenseCategories { get; set; } = new();
}
