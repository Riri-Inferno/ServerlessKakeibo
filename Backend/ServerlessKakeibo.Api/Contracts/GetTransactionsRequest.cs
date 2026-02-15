using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// 取引一覧取得リクエスト
/// </summary>
public class GetTransactionsRequest
{
    /// <summary>
    /// ページ番号(1始まり)
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    /// <summary>
    /// 1ページあたりの件数
    /// </summary>
    [Range(1, 100)]
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// 開始日(この日以降の取引を取得)
    /// </summary>
    public DateTimeOffset? StartDate { get; set; }

    /// <summary>
    /// 終了日(この日以前の取引を取得)
    /// </summary>
    public DateTimeOffset? EndDate { get; set; }

    /// <summary>
    /// カテゴリでフィルタ（後方互換用）
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionCategory? Category { get; set; }

    /// <summary>
    /// ユーザー取引カテゴリIDでフィルタ（カスタムカテゴリ対応）
    /// </summary>
    public Guid? UserTransactionCategoryId { get; set; }

    /// <summary>
    /// 支払者フィルタ（部分一致）
    /// </summary>
    public string? Payer { get; set; }

    /// <summary>
    /// 受取者(店舗名)で部分一致検索
    /// </summary>
    [MaxLength(200)]
    public string? Payee { get; set; }

    /// <summary>
    /// 最小金額
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// 最大金額
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// 取引種別でフィルタ
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransactionType? Type { get; set; }
}
