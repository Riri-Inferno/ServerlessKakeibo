using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Domain.Transaction.Policies;

/// <summary>
/// 取引レシート画像に関するポリシー
/// </summary>
public static class TransactionReceiptPolicy
{
    /// <summary>
    /// 画像添付の猶予期間（日数）
    /// </summary>
    public const int GracePeriodDays = 7;

    /// <summary>
    /// 画像を添付可能かチェック
    /// </summary>
    public static TransactionValidationResult CanAttachReceipt(TransactionEntity transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        // 既に画像が添付されている場合
        if (!string.IsNullOrEmpty(transaction.SourceUrl))
        {
            return TransactionValidationResult.Failure(
                ErrorSeverity.Critical,
                "この取引には既に画像が添付されています。" +
                "セキュリティ上の理由により、画像の変更はできません。");
        }

        // 猶予期間チェック
        var gracePeriod = TimeSpan.FromDays(GracePeriodDays);
        var elapsed = DateTimeOffset.UtcNow - transaction.CreatedAt;

        if (elapsed > gracePeriod)
        {
            return TransactionValidationResult.Failure(
                ErrorSeverity.Critical,
                $"取引作成から{GracePeriodDays}日を超えています。" +
                "画像の添付期限が切れました。");
        }

        return TransactionValidationResult.Success();
    }

    /// <summary>
    /// 画像添付の期限日時を取得
    /// </summary>
    public static DateTimeOffset GetAttachmentDeadline(TransactionEntity transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        return transaction.CreatedAt.AddDays(GracePeriodDays);
    }

    /// <summary>
    /// 画像添付が可能な残り時間を取得
    /// </summary>
    public static TimeSpan GetRemainingTime(TransactionEntity transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));

        var deadline = GetAttachmentDeadline(transaction);
        var remaining = deadline - DateTimeOffset.UtcNow;

        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
}
