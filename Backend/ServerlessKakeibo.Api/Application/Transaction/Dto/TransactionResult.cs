using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Application.Transaction.Dto;

/// <summary>
/// 取引処理結果（作成/更新共通）
/// </summary>
public class TransactionResult
{
    /// <summary>
    /// 取引ID
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// 取引日
    /// </summary>
    public DateTimeOffset TransactionDate { get; set; }

    /// <summary>
    /// 取引金額合計
    /// </summary>
    public decimal AmountTotal { get; set; }

    /// <summary>
    /// 通貨コード
    /// </summary>
    public string Currency { get; set; } = "JPY";

    /// <summary>
    /// 受取者
    /// </summary>
    public string? Payee { get; set; }

    /// <summary>
    /// カテゴリ
    /// </summary>
    public TransactionCategory Category { get; set; }

    /// <summary>
    /// 処理日時
    /// </summary>
    public DateTimeOffset ProcessedAt { get; set; }

    /// <summary>
    /// 検証時の警告リスト
    /// </summary>
    public List<string> ValidationWarnings { get; set; } = new();
}
