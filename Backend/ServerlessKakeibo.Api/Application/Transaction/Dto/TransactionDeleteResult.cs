namespace ServerlessKakeibo.Api.Application.Transaction.Dto;

/// <summary>
/// 取引削除結果
/// </summary>
public class TransactionDeleteResult
{
    /// <summary>
    /// 取引ID
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// 処理日時
    /// </summary>
    public DateTimeOffset ProcessedAt { get; set; }
}
