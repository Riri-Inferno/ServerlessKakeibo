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
        var errors = new List<string>();

        // 基本的な検証
        if (!transaction.IsValid())
        {
            errors.Add("取引データが無効です");
        }

        // 税額の検証
        if (!transaction.AreTaxesValid())
        {
            errors.Add("税額の整合性が取れていません");
        }

        // 取引項目の検証
        foreach (var item in transaction.Items)
        {
            if (!item.IsAmountValid())
            {
                errors.Add($"項目「{item.Name}」の金額が不正です");
            }
        }

        return new ValidationResult(errors.Count == 0, errors);
    }

    /// <summary>
    /// カテゴリを推定
    /// </summary>
    public string SuggestCategory(Models.Transaction transaction)
    {
        // 店舗名からカテゴリを推定
        var shopName = transaction.ShopDetails?.RegisteredBusinessName
                       ?? transaction.Payee
                       ?? string.Empty;

        if (shopName.Contains("スーパー") || shopName.Contains("マート"))
            return "食費";

        if (shopName.Contains("ドラッグ") || shopName.Contains("薬局"))
            return "日用品";

        if (shopName.Contains("ガソリン") || shopName.Contains("GS"))
            return "交通費";

        if (shopName.Contains("レストラン") || shopName.Contains("カフェ"))
            return "外食";

        // 項目名から推定
        var itemNames = transaction.Items.Select(i => i.Name ?? "").ToList();

        if (itemNames.Any(n => n.Contains("電車") || n.Contains("バス")))
            return "交通費";

        if (itemNames.Any(n => n.Contains("書籍") || n.Contains("雑誌")))
            return "教育・教養";

        return "その他";
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
public record ValidationResult(bool IsValid, List<string> Errors);

/// <summary>
/// 月次サマリー
/// </summary>
public record MonthlySummary(int Year, int Month, decimal TotalAmount, int TransactionCount);
