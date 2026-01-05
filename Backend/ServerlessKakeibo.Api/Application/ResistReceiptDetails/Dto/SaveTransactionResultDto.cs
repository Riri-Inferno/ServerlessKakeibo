namespace ServerlessKakeibo.Api.Application.ResistReceiptDetails.Dto;

/// <summary>
/// 取引保存結果DTO
/// </summary>
public class SaveTransactionResultDto
{
    /// <summary>
    /// 保存された取引ID
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// 取引日時
    /// </summary>
    public DateTimeOffset? TransactionDate { get; set; }

    /// <summary>
    /// 合計金額
    /// </summary>
    public decimal? AmountTotal { get; set; }

    /// <summary>
    /// 通貨コード
    /// </summary>
    public string Currency { get; set; } = "JPY";

    /// <summary>
    /// 受取者（店舗名など）
    /// </summary>
    public string? Payee { get; set; }

    /// <summary>
    /// 保存日時
    /// </summary>
    public DateTimeOffset SavedAt { get; set; }

    /// <summary>
    /// 推定カテゴリ
    /// </summary>
    public string? SuggestedCategory { get; set; }

    /// <summary>
    /// 推定カテゴリID
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// 検証時の警告リスト
    /// </summary>
    public List<string> ValidationWarnings { get; set; } = new();
}
