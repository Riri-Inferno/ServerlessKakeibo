using ServerlessKakeibo.Api.Application.TransactionSummary.Dto;

namespace ServerlessKakeibo.Api.Application.Statistics.Dto;

/// <summary>
/// カテゴリ別支出内訳結果
/// </summary>
public class CategoryBreakdownResult
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
    /// 支出合計
    /// </summary>
    public decimal TotalExpense { get; set; }

    /// <summary>
    /// 全カテゴリの内訳（割合込み）
    /// </summary>
    public List<CategorySummary> Categories { get; set; } = new();
}
