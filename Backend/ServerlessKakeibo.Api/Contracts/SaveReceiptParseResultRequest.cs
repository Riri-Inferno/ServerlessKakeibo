using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto.Enum;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// 領収書解析結果保存リクエスト
/// </summary>
public class SaveReceiptParseResultRequest
{
    /// <summary>
    /// 領収書種別
    /// </summary>
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ReceiptType ReceiptType { get; set; }

    /// <summary>
    /// LLMによる判定信頼度(0.0 - 1.0)
    /// </summary>
    [Range(0.0, 1.0)]
    public decimal Confidence { get; set; }

    /// <summary>
    /// 正規化された取引情報
    /// </summary>
    [Required]
    public NormalizedTransactionRequest Normalized { get; set; } = default!;

    /// <summary>
    /// 解析の詳細ステータス
    /// </summary>
    [Required]
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

    /// <summary>
    /// カテゴリ(ユーザー指定)
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionCategory? Category { get; set; }
}

/// <summary>
/// 正規化された取引情報リクエスト
/// </summary>
public class NormalizedTransactionRequest
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
    /// 通貨コード(ISO 4217)
    /// </summary>
    [MaxLength(3)]
    public string Currency { get; set; } = "JPY";

    /// <summary>
    /// 支払者
    /// </summary>
    [MaxLength(200)]
    public string? Payer { get; set; }

    /// <summary>
    /// 受取者
    /// </summary>
    [MaxLength(200)]
    public string? Payee { get; set; }

    /// <summary>
    /// 支払方法
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Application.ReceiptParsing.Dto.Enum.PaymentMethod? PaymentMethod { get; set; }

    /// <summary>
    /// 税情報リスト(複数税率対応)
    /// </summary>
    public List<TaxInfoRequest> Taxes { get; set; } = new();

    /// <summary>
    /// 取引項目一覧
    /// </summary>
    public List<TransactionItemRequest> Items { get; set; } = new();

    /// <summary>
    /// 店舗詳細情報
    /// </summary>
    public ShopDetailsRequest? ShopDetails { get; set; }
}

/// <summary>
/// 取引項目リクエスト
/// </summary>
public class TransactionItemRequest
{
    /// <summary>
    /// 項目名
    /// </summary>
    [MaxLength(200)]
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
/// 税情報リクエスト
/// </summary>
public class TaxInfoRequest
{
    /// <summary>
    /// 税率(パーセンテージ)
    /// </summary>
    public int? TaxRate { get; set; }

    /// <summary>
    /// 税額
    /// </summary>
    public decimal? TaxAmount { get; set; }

    /// <summary>
    /// 課税対象額(税抜金額)
    /// </summary>
    public decimal? TaxableAmount { get; set; }
}

/// <summary>
/// 店舗情報リクエスト
/// </summary>
public class ShopDetailsRequest
{
    /// <summary>
    /// 店舗名
    /// </summary>
    [MaxLength(200)]
    public string? Name { get; set; }

    /// <summary>
    /// 支店名
    /// </summary>
    [MaxLength(200)]
    public string? Branch { get; set; }

    /// <summary>
    /// 電話番号
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 住所
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// 郵便番号
    /// </summary>
    [MaxLength(10)]
    public string? PostalCode { get; set; }

    /// <summary>
    /// インボイス登録番号
    /// </summary>
    [MaxLength(20)]
    public string? InvoiceRegistrationNumber { get; set; }

    /// <summary>
    /// 登録事業者名
    /// </summary>
    [MaxLength(200)]
    public string? RegisteredBusinessName { get; set; }
}
