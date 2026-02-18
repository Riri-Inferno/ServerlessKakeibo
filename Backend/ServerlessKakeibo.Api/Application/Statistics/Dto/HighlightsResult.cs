namespace ServerlessKakeibo.Api.Application.Statistics.Dto;

/// <summary>
/// 月次ハイライト結果
/// </summary>
public class HighlightsResult
{
    /// <summary>
    /// 最高額の支出取引
    /// </summary>
    public TransactionHighlight? MaxExpenseTransaction { get; set; }

    /// <summary>
    /// 最も頻度の高いカテゴリ
    /// </summary>
    public CategoryFrequency? MostFrequentCategory { get; set; }

    /// <summary>
    /// 1日あたりの平均支出
    /// </summary>
    public decimal AverageExpensePerDay { get; set; }

    /// <summary>
    /// 支出があった日数
    /// </summary>
    public int DaysWithExpense { get; set; }
}

/// <summary>
/// 取引ハイライト
/// </summary>
public class TransactionHighlight
{
    /// <summary>
    /// 取引ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 支払先
    /// </summary>
    public string Payee { get; set; } = string.Empty;

    /// <summary>
    /// 金額
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 取引日
    /// </summary>
    public DateTimeOffset TransactionDate { get; set; }

    /// <summary>
    /// カテゴリID
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// カテゴリ名
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// カラーコード
    /// </summary>
    public string ColorCode { get; set; } = string.Empty;
}

/// <summary>
/// カテゴリ頻度
/// </summary>
public class CategoryFrequency
{
    /// <summary>
    /// カテゴリID
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// カテゴリ名
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// カラーコード
    /// </summary>
    public string ColorCode { get; set; } = string.Empty;

    /// <summary>
    /// 取引件数
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// 合計金額
    /// </summary>
    public decimal TotalAmount { get; set; }
}
