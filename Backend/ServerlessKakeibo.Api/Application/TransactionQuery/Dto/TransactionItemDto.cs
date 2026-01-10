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
    /// 商品カテゴリ
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ItemCategory Category { get; set; }
}
