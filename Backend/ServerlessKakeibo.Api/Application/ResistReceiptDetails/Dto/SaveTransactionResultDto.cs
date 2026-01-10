using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Application.registReceiptDetails.Dto;

/// <summary>
/// 取引保存結果DTO
/// </summary>
public class SaveTransactionResultDto
{
    /// <summary>
    /// 取引ID
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// 取引日
    /// </summary>
    public DateTimeOffset? TransactionDate { get; set; }

    /// <summary>
    /// 取引金額
    /// </summary>
    public decimal? AmountTotal { get; set; }

    /// <summary>
    /// 通貨コード
    /// </summary>
    public string Currency { get; set; } = "JPY";

    /// <summary>
    /// 受取者
    /// </summary>
    public string? Payee { get; set; }

    /// <summary>
    /// 保存日時
    /// </summary>
    public DateTimeOffset SavedAt { get; set; }

    /// <summary>
    /// カテゴリ(保存された値)
    /// </summary>
    public TransactionCategory Category { get; set; }

    /// <summary>
    /// 検証時の警告リスト
    /// </summary>
    public List<string> ValidationWarnings { get; set; } = new();
}
