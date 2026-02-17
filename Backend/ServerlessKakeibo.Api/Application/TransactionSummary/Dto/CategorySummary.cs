namespace ServerlessKakeibo.Api.Application.TransactionSummary.Dto;

/// <summary>
/// カテゴリ別サマリー
/// </summary>
public class CategorySummary
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
    /// カラーコード（例: #FF5733）
    /// </summary>
    public string ColorCode { get; set; } = string.Empty;

    /// <summary>
    /// 金額合計
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 取引件数
    /// </summary>
    public int Count { get; set; }
}
