using System.Text.Json.Serialization;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Application.TransactionQuery.Dto;

/// <summary>
/// 取引サマリー結果(一覧用の軽量版)
/// </summary>
public class TransactionSummaryResult
{
    /// <summary>
    /// 取引ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 取引タイプ
    /// </summary>
    public TransactionType Type { get; set; }

    /// <summary>
    /// 取引日時
    /// </summary>
    public DateTimeOffset? TransactionDate { get; set; }

    /// <summary>
    /// 取引金額合計
    /// </summary>
    public decimal? AmountTotal { get; set; }

    /// <summary>
    /// 通貨コード
    /// </summary>
    public string Currency { get; set; } = "JPY";

    /// <summary>
    /// 受取者(店舗名など)
    /// </summary>
    public string? Payee { get; set; }

    /// <summary>
    /// カテゴリ
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionCategory Category { get; set; }

    /// <summary>
    /// 支払方法
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// 税の扱い（外税・内税・不明）
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaxInclusionType? TaxInclusionType { get; set; }

    /// <summary>
    /// 取引項目数
    /// </summary>
    public int ItemCount { get; set; }
}
