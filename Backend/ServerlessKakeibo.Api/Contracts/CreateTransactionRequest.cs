using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// 取引新規作成リクエスト
/// </summary>
public class CreateTransactionRequest : IValidatableObject
{
    /// <summary>
    /// 取引種別（収入/支出）
    /// </summary>
    [Required(ErrorMessage = "取引種別は必須です")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionType Type { get; set; } = TransactionType.Expense;

    /// <summary>
    /// 取引日時
    /// </summary>
    [Required(ErrorMessage = "取引日時は必須です")]
    public DateTimeOffset TransactionDate { get; set; }

    /// <summary>
    /// 取引金額合計（クライアント指定値を優先）
    /// </summary>
    [Required(ErrorMessage = "取引金額は必須です")]
    [Range(0.01, double.MaxValue, ErrorMessage = "取引金額は0より大きい値を指定してください")]
    public decimal AmountTotal { get; set; }

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
    /// ユーザー取引カテゴリID（カスタムカテゴリ対応）
    /// </summary>
    public Guid? UserTransactionCategoryId { get; set; }

    /// <summary>
    /// メモ・備考
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// 税の扱い（外税・内税・不明）※フロントエンドが指定
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaxInclusionType? TaxInclusionType { get; set; }

    /// <summary>
    /// 取引項目一覧
    /// </summary>
    public List<CreateTransactionItemRequest> Items { get; set; } = new();

    /// <summary>
    /// 税情報一覧
    /// </summary>
    public List<CreateTaxDetailRequest> Taxes { get; set; } = new();

    /// <summary>
    /// 店舗詳細情報
    /// </summary>
    public CreateShopDetailRequest? ShopDetails { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // 支出の場合は Items が必須
        if (Type == TransactionType.Expense && (Items == null || Items.Count == 0))
        {
            yield return new ValidationResult(
                "支出の場合、取引項目は最低1件必要です",
                new[] { nameof(Items) });
        }

        // 収入の場合は AmountTotal が必須（Items は任意）
        if (Type == TransactionType.Income && AmountTotal <= 0)
        {
            yield return new ValidationResult(
                "収入の場合、取引金額は必須です",
                new[] { nameof(AmountTotal) });
        }
    }
}

/// <summary>
/// 取引項目作成リクエスト
/// </summary>
public class CreateTransactionItemRequest : IValidatableObject
{
    /// <summary>
    /// 項目種別（商品/値引き）。既定は Product。
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionItemType ItemType { get; set; } = TransactionItemType.Product;

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
    /// 単価（値引きの場合はマイナス値を許容）
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// 金額（値引きの場合はマイナス値）
    /// </summary>
    [Required(ErrorMessage = "金額は必須です")]
    public decimal Amount { get; set; }

    /// <summary>
    /// ユーザー商品カテゴリID（支出用・カスタムカテゴリ対応）
    /// </summary>
    public Guid? UserItemCategoryId { get; set; }

    /// <summary>
    /// ユーザー収入項目カテゴリID（収入用・カスタムカテゴリ対応）
    /// </summary>
    public Guid? UserIncomeItemCategoryId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ItemType == TransactionItemType.Discount)
        {
            if (Amount > 0)
            {
                yield return new ValidationResult(
                    "値引き項目の金額は0以下で指定してください",
                    new[] { nameof(Amount) });
            }
            if (UnitPrice.HasValue && UnitPrice.Value > 0)
            {
                yield return new ValidationResult(
                    "値引き項目の単価は0以下で指定してください",
                    new[] { nameof(UnitPrice) });
            }
        }
        else // Product
        {
            if (Amount < 0)
            {
                yield return new ValidationResult(
                    "金額は0以上を指定してください",
                    new[] { nameof(Amount) });
            }
            if (UnitPrice.HasValue && UnitPrice.Value < 0)
            {
                yield return new ValidationResult(
                    "単価は0以上を指定してください",
                    new[] { nameof(UnitPrice) });
            }
        }
    }
}

/// <summary>
/// 税情報作成リクエスト
/// </summary>
public class CreateTaxDetailRequest
{
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
/// 店舗詳細情報作成リクエスト
/// </summary>
public class CreateShopDetailRequest
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
