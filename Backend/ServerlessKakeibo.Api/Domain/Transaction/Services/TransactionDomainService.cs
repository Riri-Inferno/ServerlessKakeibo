using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Domain.Transaction.Services;

/// <summary>
/// 取引ドメインサービス
/// </summary>
public class TransactionDomainService
{
    /// <summary>
    /// 取引の整合性を検証
    /// </summary>
    public ValidationResult ValidateTransaction(Models.Transaction transaction)
    {
        var errors = new List<ValidationError>();

        // 基本的な検証
        if (!transaction.IsValid())
        {
            errors.Add(new ValidationError(
                "取引データが無効です",
                ErrorSeverity.Critical));
        }

        // 必須項目チェック
        if (!transaction.TransactionDate.HasValue)
        {
            errors.Add(new ValidationError(
                "取引日が設定されていません",
                ErrorSeverity.Critical));
        }

        if (!transaction.AmountTotal.HasValue || transaction.AmountTotal <= 0)
        {
            errors.Add(new ValidationError(
                "取引金額が不正です",
                ErrorSeverity.Critical));
        }

        // 税額の検証
        if (!transaction.AreTaxesValid())
        {
            errors.Add(new ValidationError(
                "税額の整合性が取れていません",
                ErrorSeverity.Warning));
        }

        // 取引項目の検証
        foreach (var item in transaction.Items)
        {
            if (!item.IsAmountValid())
            {
                errors.Add(new ValidationError(
                    $"項目「{item.Name}」の金額が不正です",
                    ErrorSeverity.Warning));
            }
        }

        // Payeeが空の場合は警告
        if (string.IsNullOrWhiteSpace(transaction.Payee))
        {
            errors.Add(new ValidationError(
                "受取者(店舗名)が設定されていません",
                ErrorSeverity.Warning));
        }

        return new ValidationResult
        {
            IsValid = !errors.Any(e => e.Severity == ErrorSeverity.Critical),
            Errors = errors
        };
    }

    /// <summary>
    /// 月次集計
    /// </summary>
    public MonthlySummary CalculateMonthlySummary(List<Models.Transaction> transactions, int year, int month)
    {
        var dateRange = DateRange.ForMonth(year, month);

        var monthlyTransactions = transactions
            .Where(t => t.TransactionDate.HasValue && dateRange.Contains(t.TransactionDate.Value))
            .ToList();

        var totalAmount = monthlyTransactions.Sum(t => t.AmountTotal ?? 0);
        var transactionCount = monthlyTransactions.Count;

        return new MonthlySummary(year, month, totalAmount, transactionCount);
    }

    /// <summary>
    /// 削除前の検証
    /// </summary>
    public DeleteValidationResult ValidateDelete(Models.Transaction transaction)
    {
        var warnings = new List<ValidationError>();

        // 情報: 高額取引の削除
        if (transaction.AmountTotal > 50000) // 5万円以上
        {
            warnings.Add(new ValidationError(
                $"高額な支出({transaction.AmountTotal:N0}円)を削除します。",
                ErrorSeverity.Info));
        }

        // 情報: 古いデータの削除(統計への影響を通知)
        if (transaction.TransactionDate.HasValue)
        {
            var monthsOld = CalculateMonthsOld(transaction.TransactionDate.Value);

            if (monthsOld >= 3) // 3ヶ月以上前
            {
                warnings.Add(new ValidationError(
                    "過去の取引を削除すると、月次統計が変わります。",
                    ErrorSeverity.Info));
            }
        }

        // 情報: 複数明細を持つ取引
        if (transaction.Items.Count > 5)
        {
            warnings.Add(new ValidationError(
                $"{transaction.Items.Count}件の明細を含む取引を削除します。",
                ErrorSeverity.Info));
        }

        return new DeleteValidationResult
        {
            CanDelete = true, // 家計簿なので常に削除可能
            Warnings = warnings
        };
    }

    /// <summary>
    /// 取引日からの経過月数を計算
    /// </summary>
    private int CalculateMonthsOld(DateTimeOffset transactionDate)
    {
        var now = DateTimeOffset.UtcNow;
        return (now.Year - transactionDate.Year) * 12
             + (now.Month - transactionDate.Month);
    }
}

/// <summary>
/// 検証結果
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
}

/// <summary>
/// 削除検証結果
/// </summary>
public class DeleteValidationResult
{
    public bool CanDelete { get; set; }
    public List<ValidationError> Warnings { get; set; } = new();
}

/// <summary>
/// 検証エラー
/// </summary>
public class ValidationError
{
    public string Message { get; set; }
    public ErrorSeverity Severity { get; set; }

    public ValidationError(string message, ErrorSeverity severity)
    {
        Message = message;
        Severity = severity;
    }
}

/// <summary>
/// 月次サマリー
/// </summary>
public record MonthlySummary(int Year, int Month, decimal TotalAmount, int TransactionCount);
