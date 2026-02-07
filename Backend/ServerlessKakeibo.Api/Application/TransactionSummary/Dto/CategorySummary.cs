using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Application.TransactionSummary.Dto;

/// <summary>
/// カテゴリ別サマリー
/// </summary>
public class CategorySummary
{
    /// <summary>
    /// カテゴリ
    /// </summary>
    public Domain.ValueObjects.TransactionCategory Category { get; set; }

    /// <summary>
    /// カテゴリ名（日本語）
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 金額合計
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// 取引件数
    /// </summary>
    public int Count { get; set; }
}
