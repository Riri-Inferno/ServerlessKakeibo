using ServerlessKakeibo.Api.Application.TransactionSummary.Dto;
using ServerlessKakeibo.Api.Application.TransactionSummary.Usecases;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.TransactionSummary;

/// <summary>
/// 月次サマリーインタラクター
/// </summary>
public class MonthlySummaryInteractor : IMonthlySummaryUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<MonthlySummaryInteractor> _logger;

    public MonthlySummaryInteractor(
        ITransactionRepository transactionRepository,
        ILogger<MonthlySummaryInteractor> logger)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 月次サマリーを取得
    /// </summary>
    public async Task<MonthlySummaryResult> GetMonthlySummaryAsync(
        int year,
        int month,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        if (year < 2000 || year > 2100)
            throw new ArgumentException("Year must be between 2000 and 2100", nameof(year));

        if (month < 1 || month > 12)
            throw new ArgumentException("Month must be between 1 and 12", nameof(month));

        try
        {
            _logger.LogInformation(
                "月次サマリー取得を開始します。UserId: {UserId}, Year: {Year}, Month: {Month}",
                userId, year, month);

            // Repository からエンティティを取得
            var transactions = await _transactionRepository
                .GetMonthlyTransactionsWithCategoryAsync(userId, year, month, cancellationToken);

            // Interactor で集計
            var totalIncome = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.AmountTotal ?? 0);

            var totalExpense = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.AmountTotal ?? 0);

            var balance = totalIncome - totalExpense;

            // カテゴリ別集計（支出）
            var expenseByCategory = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => t.UserTransactionCategory!.Id)
                .Select(g => new CategorySummary
                {
                    CategoryId = g.Key,
                    CategoryName = g.First().UserTransactionCategory!.Name,
                    ColorCode = g.First().UserTransactionCategory!.ColorCode,
                    Amount = g.Sum(t => t.AmountTotal ?? 0),
                    Count = g.Count()
                })
                .OrderByDescending(c => c.Amount)
                .ToList();

            // カテゴリ別集計（収入）
            var incomeByCategory = transactions
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => t.UserTransactionCategory!.Id)
                .Select(g => new CategorySummary
                {
                    CategoryId = g.Key,
                    CategoryName = g.First().UserTransactionCategory!.Name,
                    ColorCode = g.First().UserTransactionCategory!.ColorCode,
                    Amount = g.Sum(t => t.AmountTotal ?? 0),
                    Count = g.Count()
                })
                .ToList();

            // 支出トップ3を抽出
            var topExpenseCategories = expenseByCategory.Take(3).ToList();

            var incomeCount = incomeByCategory.Sum(c => c.Count);
            var expenseCount = expenseByCategory.Sum(c => c.Count);
            var totalCount = incomeCount + expenseCount;

            _logger.LogInformation(
                "月次サマリーを取得しました。UserId: {UserId}, Income: {Income}, Expense: {Expense}, Balance: {Balance}",
                userId, totalIncome, totalExpense, balance);

            return new MonthlySummaryResult
            {
                Year = year,
                Month = month,
                Income = totalIncome,
                Expense = totalExpense,
                Balance = balance,
                TransactionCount = totalCount,
                IncomeCount = incomeCount,
                ExpenseCount = expenseCount,
                TopExpenseCategories = topExpenseCategories
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "月次サマリー取得中にエラーが発生しました。UserId: {UserId}, Year: {Year}, Month: {Month}",
                userId, year, month);
            throw;
        }
    }
}
