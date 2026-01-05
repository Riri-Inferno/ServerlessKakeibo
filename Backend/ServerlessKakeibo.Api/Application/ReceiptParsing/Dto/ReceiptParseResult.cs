using System.Text.Json;
using System.Text.Json.Serialization;
using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto.Enum;
using ServerlessKakeibo.Api.Domain.Receipt.Models;

namespace ServerlessKakeibo.Api.Application.ReceiptParsing.Dto;

/// <summary>
/// 領収書解析結果のレスポンスデータ
/// </summary>
public class ReceiptParseResult
{
    /// <summary>
    /// 領収書種別
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReceiptType ReceiptType { get; set; }

    /// <summary>
    /// LLMによる判定信頼度（0.0 - 1.0）
    /// </summary>
    public decimal Confidence { get; set; }

    /// <summary>
    /// 正規化された取引情報
    /// </summary>
    public NormalizedTransaction Normalized { get; set; } = default!;

    /// <summary>
    /// 元帳票依存の生データ（デバッグ・再解析用）
    /// </summary>
    public JsonElement? Raw { get; set; }

    /// <summary>
    /// 解析の詳細ステータス
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ParseStatus ParseStatus { get; set; }

    /// <summary>
    /// 警告メッセージリスト
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// 欠落フィールドリスト
    /// </summary>
    public List<string> MissingFields { get; set; } = new();
}

/// <summary>
/// 正規化された取引情報
/// </summary>
public class NormalizedTransaction
{
    /// <summary>
    /// 取引日
    /// </summary>
    public DateTimeOffset? TransactionDate { get; set; }

    /// <summary>
    /// 取引金額
    /// </summary>
    public decimal? AmountTotal { get; set; }

    /// <summary>
    /// 通貨コード（ISO 4217）
    /// </summary>
    public string Currency { get; set; } = "JPY";

    /// <summary>
    /// 支払者
    /// </summary>
    public string? Payer { get; set; }

    /// <summary>
    /// 受取者
    /// </summary>
    public string? Payee { get; set; }

    /// <summary>
    /// 支払方法
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// 税情報リスト（複数税率対応）
    /// </summary>
    public List<TaxInfo> Taxes { get; set; } = new();

    /// <summary>
    /// 取引項目一覧
    /// </summary>
    public List<NormalizedItem> Items { get; set; } = new();

    /// <summary>
    /// 店舗詳細情報
    /// </summary>
    public ShopDetails? ShopDetails { get; set; }
}

/// <summary>
/// 正規化された取引項目
/// </summary>
public class NormalizedItem
{
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
}

/// <summary>
/// 税情報（DTO）
/// </summary>
public class TaxInfo
{
    /// <summary>
    /// 税率（パーセンテージ）
    /// </summary>
    public int? TaxRate { get; set; }

    /// <summary>
    /// 税額
    /// </summary>
    public decimal? TaxAmount { get; set; }

    /// <summary>
    /// 課税対象額（税抜金額）
    /// </summary>
    public decimal? TaxableAmount { get; set; }
}
