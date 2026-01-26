using ServerlessKakeibo.Api.Application.Statistics.Dto;
using ServerlessKakeibo.Api.Application.Statistics.Usecases;
using ServerlessKakeibo.Api.Application.TransactionSummary.Dto;
using ServerlessKakeibo.Api.Application.TransactionSummary.Usecases;
using ServerlessKakeibo.Api.Common.Helpers;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.Statistics;

/// <summary>
/// 統計情報インタラクター
/// </summary>
public class StatisticsInteractor : IStatisticsUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IMonthlySummaryUseCase _monthlySummaryUseCase;
    private readonly ILogger<StatisticsInteractor> _logger;

    public StatisticsInteractor(
        ITransactionRepository transactionRepository,
        IMonthlySummaryUseCase monthlySummaryUseCase,
        ILogger<StatisticsInteractor> logger)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _monthlySummaryUseCase = monthlySummaryUseCase ?? throw new ArgumentNullException(nameof(monthlySummaryUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 前月比込みの月次サマリーを取得
    /// </summary>
    public async Task<MonthlyComparisonResult> GetMonthlyComparisonAsync(
        int year,
        int month,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateCommonParameters(userId, year, month);

        try
        {
            _logger.LogInformation(
                "前月比込みサマリー取得を開始します。UserId: {UserId}, Year: {Year}, Month: {Month}",
                userId, year, month);

            // 当月データを取得
            var current = await _monthlySummaryUseCase.GetMonthlySummaryAsync(
                year, month, userId, cancellationToken);

            // 前月を計算
            var prevMonth = month == 1 ? 12 : month - 1;
            var prevYear = month == 1 ? year - 1 : year;

            // 前月データを取得
            MonthlySummaryResult? previous = null;
            try
            {
                previous = await _monthlySummaryUseCase.GetMonthlySummaryAsync(
                    prevYear, prevMonth, userId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "前月データの取得に失敗しました。前月比計算をスキップします。UserId: {UserId}, Year: {Year}, Month: {Month}",
                    userId, prevYear, prevMonth);
            }

            // 前月比を計算
            var incomeChangePercent = MathHelper.CalculatePercentChange(previous?.Income ?? 0, current.Income);
            var expenseChangePercent = MathHelper.CalculatePercentChange(previous?.Expense ?? 0, current.Expense);
            var balanceChangePercent = MathHelper.CalculatePercentChange(previous?.Balance ?? 0, current.Balance);

            _logger.LogInformation(
                "前月比込みサマリーを取得しました。UserId: {UserId}, IncomeChange: {IncomeChange}%, ExpenseChange: {ExpenseChange}%",
                userId, incomeChangePercent, expenseChangePercent);

            return new MonthlyComparisonResult
            {
                Current = current,
                Previous = previous,
                IncomeChangePercent = incomeChangePercent,
                ExpenseChangePercent = expenseChangePercent,
                BalanceChangePercent = balanceChangePercent
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "前月比込みサマリー取得中にエラーが発生しました。UserId: {UserId}, Year: {Year}, Month: {Month}",
                userId, year, month);
            throw;
        }
    }

    /// <summary>
    /// カテゴリ別支出内訳を取得（全カテゴリ）
    /// </summary>
    public async Task<CategoryBreakdownResult> GetCategoryBreakdownAsync(
        int year,
        int month,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateCommonParameters(userId, year, month);

        try
        {
            _logger.LogInformation(
                "カテゴリ別支出内訳取得を開始します。UserId: {UserId}, Year: {Year}, Month: {Month}",
                userId, year, month);

            var expenseByCategory = await _transactionRepository.GetAllCategoryExpensesAsync(
                userId, year, month, cancellationToken);

            var totalExpense = expenseByCategory.Values.Sum(x => x.Amount);

            // 全カテゴリを金額順にソート
            var categories = expenseByCategory
                .OrderByDescending(x => x.Value.Amount)
                .Select(x => new CategorySummary
                {
                    Category = x.Key,
                    CategoryName = x.Key.ToJapanese(),
                    Amount = x.Value.Amount,
                    Count = x.Value.Count
                })
                .ToList();

            _logger.LogInformation(
                "カテゴリ別支出内訳を取得しました。UserId: {UserId}, TotalExpense: {TotalExpense}, CategoryCount: {CategoryCount}",
                userId, totalExpense, categories.Count);

            return new CategoryBreakdownResult
            {
                Year = year,
                Month = month,
                TotalExpense = totalExpense,
                Categories = categories
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "カテゴリ別支出内訳取得中にエラーが発生しました。UserId: {UserId}, Year: {Year}, Month: {Month}",
                userId, year, month);
            throw;
        }
    }

    /// <summary>
    /// 月次推移を取得（直近N ヶ月）
    /// </summary>
    public async Task<MonthlyTrendResult> GetMonthlyTrendAsync(
        int months,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (months < 1 || months > 24)
            throw new ArgumentException("Months must be between 1 and 24", nameof(months));

        try
        {
            _logger.LogInformation(
                "月次推移取得を開始します。UserId: {UserId}, Months: {Months}",
                userId, months);

            // 直近N ヶ月の年月リストを生成
            var now = DateTimeOffset.UtcNow;
            var monthlyRanges = new List<(int Year, int Month)>();

            for (int i = months - 1; i >= 0; i--)
            {
                var targetDate = now.AddMonths(-i);
                monthlyRanges.Add((targetDate.Year, targetDate.Month));
            }

            // 一括で月次集計を取得
            var aggregates = await _transactionRepository.GetMonthlyAggregatesAsync(
                userId, monthlyRanges, cancellationToken);

            // 結果を整形
            var monthLabels = aggregates.Select(x => new MonthLabel
            {
                Year = x.Year,
                Month = x.Month,
                Label = $"{x.Year}年{x.Month}月"
            }).ToList();

            var incomes = aggregates.Select(x => x.Income).ToList();
            var expenses = aggregates.Select(x => x.Expense).ToList();
            var balances = aggregates.Select(x => x.Balance).ToList();

            _logger.LogInformation(
                "月次推移を取得しました。UserId: {UserId}, Months: {Months}",
                userId, months);

            return new MonthlyTrendResult
            {
                Months = monthLabels,
                Incomes = incomes,
                Expenses = expenses,
                Balances = balances
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "月次推移取得中にエラーが発生しました。UserId: {UserId}, Months: {Months}",
                userId, months);
            throw;
        }
    }

    /// <summary>
    /// 月次ハイライトを取得
    /// </summary>
    public async Task<HighlightsResult> GetHighlightsAsync(
        int year,
        int month,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateCommonParameters(userId, year, month);

        try
        {
            _logger.LogInformation(
                "月次ハイライト取得を開始します。UserId: {UserId}, Year: {Year}, Month: {Month}",
                userId, year, month);

            var maxExpenseTransaction = await _transactionRepository.GetMaxExpenseTransactionAsync(
                userId, year, month, cancellationToken);

            var mostFrequentCategory = await _transactionRepository.GetMostFrequentCategoryAsync(
                userId, year, month, cancellationToken);

            var daysWithExpense = await _transactionRepository.GetDaysWithExpenseAsync(
                userId, year, month, cancellationToken);

            var (_, totalExpense, _) = await _transactionRepository.GetMonthlySummaryAsync(
                userId, year, month, cancellationToken);

            // 平均支出を計算
            var averageExpensePerDay = daysWithExpense > 0
                ? totalExpense / daysWithExpense
                : 0;

            // 最高額取引を整形
            TransactionHighlight? maxExpenseHighlight = null;
            if (maxExpenseTransaction != null)
            {
                maxExpenseHighlight = new TransactionHighlight
                {
                    Id = maxExpenseTransaction.Id,
                    Payee = maxExpenseTransaction.Payee ?? "不明",
                    Amount = maxExpenseTransaction.AmountTotal ?? 0,
                    TransactionDate = maxExpenseTransaction.TransactionDate ?? DateTimeOffset.UtcNow,
                    Category = maxExpenseTransaction.Category.ToString(),
                    CategoryName = maxExpenseTransaction.Category.ToJapanese()
                };
            }

            // 最多カテゴリを整形
            CategoryFrequency? mostFrequentFrequency = null;
            if (mostFrequentCategory.HasValue)
            {
                var (category, count, totalAmount) = mostFrequentCategory.Value;
                mostFrequentFrequency = new CategoryFrequency
                {
                    Category = category.ToString(),
                    CategoryName = category.ToJapanese(),
                    Count = count,
                    TotalAmount = totalAmount
                };
            }

            _logger.LogInformation(
                "月次ハイライトを取得しました。UserId: {UserId}, MaxExpense: {MaxExpense}, DaysWithExpense: {DaysWithExpense}",
                userId, maxExpenseHighlight?.Amount ?? 0, daysWithExpense);

            return new HighlightsResult
            {
                MaxExpenseTransaction = maxExpenseHighlight,
                MostFrequentCategory = mostFrequentFrequency,
                AverageExpensePerDay = averageExpensePerDay,
                DaysWithExpense = daysWithExpense
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "月次ハイライト取得中にエラーが発生しました。UserId: {UserId}, Year: {Year}, Month: {Month}",
                userId, year, month);
            throw;
        }
    }

    /// <summary>
    /// 共通パラメータのバリデーション
    /// </summary>
    private static void ValidateCommonParameters(Guid userId, int year, int month)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (year < 2000 || year > 2100)
            throw new ArgumentException("Year must be between 2000 and 2100", nameof(year));

        if (month < 1 || month > 12)
            throw new ArgumentException("Month must be between 1 and 12", nameof(month));
    }
}
