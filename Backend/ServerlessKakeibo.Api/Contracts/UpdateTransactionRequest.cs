using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// 取引更新リクエスト
/// </summary>
public class UpdateTransactionRequest
{
    /// <summary>
    /// 取引日時
    /// </summary>
    [Required(ErrorMessage = "取引日時は必須です")]
    public DateTimeOffset TransactionDate { get; set; }

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
    /// 受取者(店舗名など)
    /// </summary>
    [MaxLength(200)]
    public string? Payee { get; set; }

    /// <summary>
    /// 支払方法
    /// </summary>
    [MaxLength(50)]
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// カテゴリ
    /// </summary>
    [Required(ErrorMessage = "カテゴリは必須です")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionCategory Category { get; set; }

    /// <summary>
    /// メモ・備考
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// 取引項目一覧（全件送信・Full Replace方式）
    /// </summary>
    public List<UpdateTransactionItemRequest> Items { get; set; } = new();

    /// <summary>
    /// 税情報一覧（全件送信・Full Replace方式）
    /// </summary>
    public List<UpdateTaxDetailRequest> Taxes { get; set; } = new();

    /// <summary>
    /// 店舗詳細情報
    /// </summary>
    public UpdateShopDetailRequest? ShopDetails { get; set; }
}

/// <summary>
/// 取引項目更新リクエスト
/// </summary>
public class UpdateTransactionItemRequest
{
    /// <summary>
    /// 項目ID（既存項目の場合は指定、新規追加の場合はnull）
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// 項目名
    /// </summary>
    [Required(ErrorMessage = "項目名は必須です")]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// 数量
    /// </summary>
    [Range(0.001, double.MaxValue, ErrorMessage = "数量は0より大きい値を指定してください")]
    public decimal Quantity { get; set; } = 1;

    /// <summary>
    /// 単価
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "単価は0以上を指定してください")]
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// 金額
    /// </summary>
    [Required(ErrorMessage = "金額は必須です")]
    [Range(0, double.MaxValue, ErrorMessage = "金額は0以上を指定してください")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 商品カテゴリ
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ItemCategory Category { get; set; } = ItemCategory.Uncategorized;
}

/// <summary>
/// 税情報更新リクエスト
/// </summary>
public class UpdateTaxDetailRequest
{
    /// <summary>
    /// 税情報ID（既存の場合は指定、新規追加の場合はnull）
    /// </summary>
    public Guid? Id { get; set; }

    /// <summary>
    /// 税率（パーセンテージ）
    /// </summary>
    [Range(0, 100, ErrorMessage = "税率は0-100の範囲で指定してください")]
    public int? TaxRate { get; set; }

    /// <summary>
    /// 税額
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "税額は0以上を指定してください")]
    public decimal? TaxAmount { get; set; }

    /// <summary>
    /// 対象金額（税抜）
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "対象金額は0以上を指定してください")]
    public decimal? TaxableAmount { get; set; }

    /// <summary>
    /// 税種別
    /// </summary>
    [MaxLength(50)]
    public string TaxType { get; set; } = "消費税";
}

/// <summary>
/// 店舗詳細情報更新リクエスト
/// </summary>
public class UpdateShopDetailRequest
{
    /// <summary>
    /// 店舗詳細ID（既存の場合は指定、新規追加の場合はnull）
    /// </summary>
    public Guid? Id { get; set; }

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
    /// 郵便番号
    /// </summary>
    [MaxLength(20)]
    public string? PostalCode { get; set; }

    /// <summary>
    /// 住所
    /// </summary>
    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>
    /// 電話番号
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}
