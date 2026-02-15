using System.Text.Json.Serialization;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Application.TransactionQuery.Dto;

/// <summary>
/// 取引詳細結果DTO
/// </summary>
public class TransactionDetailResult
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
    /// 通貨コード(ISO 4217)
    /// </summary>
    public string Currency { get; set; } = "JPY";

    /// <summary>
    /// 支払者
    /// </summary>
    public string? Payer { get; set; }

    /// <summary>
    /// 受取者(店舗名など)
    /// </summary>
    public string? Payee { get; set; }

    /// <summary>
    /// 支払方法
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// カテゴリ:後方互換一掃
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Domain.ValueObjects.TransactionCategory Category { get; set; }

    /// <summary>
    /// ユーザー取引カテゴリ（カスタムカテゴリ対応）
    /// </summary>
    public UserTransactionCategoryDto? UserTransactionCategory { get; set; }

    /// <summary>
    /// メモ・備考
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// 税の扱い（外税・内税・不明）
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TaxInclusionType? TaxInclusionType { get; set; }

    /// <summary>
    /// 元帳票画像のストレージパス（GCS上のフルパス）
    /// </summary>
    public string? SourceUrl { get; set; }

    /// <summary>
    /// 画像添付日時（null = 未添付）
    /// </summary>
    public DateTimeOffset? ReceiptAttachedAt { get; set; }

    /// <summary>
    /// 領収書種別
    /// </summary>
    public string? ReceiptType { get; set; }

    /// <summary>
    /// LLMによる判定信頼度(0.0 - 1.0)
    /// </summary>
    public decimal? Confidence { get; set; }

    /// <summary>
    /// 解析ステータス
    /// </summary>
    public string? ParseStatus { get; set; }

    /// <summary>
    /// 警告メッセージ
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// 欠落フィールド
    /// </summary>
    public List<string> MissingFields { get; set; } = new();

    /// <summary>
    /// 取引項目一覧
    /// </summary>
    public List<TransactionItemDto> Items { get; set; } = new();

    /// <summary>
    /// 税情報一覧
    /// </summary>
    public List<TaxDetailDto> Taxes { get; set; } = new();

    /// <summary>
    /// 店舗詳細情報
    /// </summary>
    public ShopDetailDto? ShopDetails { get; set; }

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 更新日時
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}
