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
