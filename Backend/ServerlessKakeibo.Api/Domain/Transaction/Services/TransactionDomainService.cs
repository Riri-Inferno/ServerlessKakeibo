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
                ErrorSeverity.Warning));  // 軽微なエラーとして扱う
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
    /// カテゴリを推定
    /// </summary>
    public CategorySuggestion? SuggestCategory(Models.Transaction transaction)
    {
        // 店舗名からカテゴリを推定
        var shopName = transaction.ShopDetails?.RegisteredBusinessName
                       ?? transaction.Payee
                       ?? string.Empty;

        var categoryName = DetermineCategoryFromShopName(shopName);

        if (categoryName == null)
        {
            // 項目名から推定
            var itemNames = transaction.Items.Select(i => i.Name ?? "").ToList();
            categoryName = DetermineCategoryFromItems(itemNames);
        }

        if (categoryName == null)
        {
            categoryName = "その他";
        }

        // カテゴリ名に対応するIDを生成(仮実装)
        var categoryId = GenerateCategoryId(categoryName);

        return new CategorySuggestion
        {
            Id = categoryId,
            Name = categoryName
        };
    }

    /// <summary>
    /// 店舗名からカテゴリを判定
    /// </summary>
    private string? DetermineCategoryFromShopName(string shopName)
    {
        if (string.IsNullOrWhiteSpace(shopName))
            return null;

        var name = shopName.ToLower();

        if (name.Contains("スーパー") || name.Contains("マート") || name.Contains("イオン"))
            return "食費";

        if (name.Contains("ドラッグ") || name.Contains("薬局") || name.Contains("ウエルシア"))
            return "日用品";

        if (name.Contains("ガソリン") || name.Contains("gs") || name.Contains("エネオス"))
            return "交通費";

        if (name.Contains("レストラン") || name.Contains("カフェ") || name.Contains("スタバ"))
            return "外食";

        if (name.Contains("コンビニ") || name.Contains("セブン") || name.Contains("ローソン"))
            return "食費";

        return null;
    }

    /// <summary>
    /// 項目名からカテゴリを判定
    /// </summary>
    private string? DetermineCategoryFromItems(List<string> itemNames)
    {
        if (!itemNames.Any())
            return null;

        var allItems = string.Join(" ", itemNames).ToLower();

        if (allItems.Contains("電車") || allItems.Contains("バス") || allItems.Contains("切符"))
            return "交通費";

        if (allItems.Contains("書籍") || allItems.Contains("雑誌") || allItems.Contains("本"))
            return "教育・教養";

        if (allItems.Contains("野菜") || allItems.Contains("肉") || allItems.Contains("魚"))
            return "食費";

        if (allItems.Contains("シャンプー") || allItems.Contains("洗剤") || allItems.Contains("ティッシュ"))
            return "日用品";

        return null;
    }

    /// <summary>
    /// カテゴリ名からIDを生成(仮実装)
    /// TODO: データベースのカテゴリマスタから取得する実装に置き換える
    /// </summary>
    private Guid GenerateCategoryId(string categoryName)
    {
        // カテゴリ名から決定的なGUIDを生成
        var categoryMap = new Dictionary<string, string>
        {
            { "食費", "10000000-0000-0000-0000-000000000001" },
            { "外食", "10000000-0000-0000-0000-000000000002" },
            { "日用品", "10000000-0000-0000-0000-000000000003" },
            { "交通費", "10000000-0000-0000-0000-000000000004" },
            { "教育・教養", "10000000-0000-0000-0000-000000000005" },
            { "その他", "10000000-0000-0000-0000-000000000099" }
        };

        return categoryMap.TryGetValue(categoryName, out var guidString)
            ? Guid.Parse(guidString)
            : Guid.Parse(categoryMap["その他"]);
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
/// カテゴリ推定結果
/// </summary>
public class CategorySuggestion
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 月次サマリー
/// </summary>
public record MonthlySummary(int Year, int Month, decimal TotalAmount, int TransactionCount);
