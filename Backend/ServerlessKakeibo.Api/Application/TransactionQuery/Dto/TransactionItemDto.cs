using System.Text.Json.Serialization;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Application.TransactionQuery.Dto;

/// <summary>
/// 取引項目DTO
/// </summary>
public class TransactionItemDto
{
    /// <summary>
    /// 項目ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 項目名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public decimal? Quantity { get; set; }

    /// <summary>
    /// 単価
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// 金額
    /// </summary>
    public decimal? Amount { get; set; }

    /// <summary>
    /// ユーザー商品カテゴリID（支出用）
    /// </summary>
    public Guid? UserItemCategoryId { get; set; }

    /// <summary>
    /// ユーザー収入項目カテゴリID（収入用）
    /// </summary>
    public Guid? UserIncomeItemCategoryId { get; set; }

    /// <summary>
    /// 明細のカテゴリ
    /// </summary>
    public UserItemCategoryDto? UserItemCategory { get; set; }

    /// <summary>
    /// 収入明細のカテゴリ
    /// </summary>
    public UserIncomeItemCategoryDto? UserIncomeItemCategory { get; set; }
}
