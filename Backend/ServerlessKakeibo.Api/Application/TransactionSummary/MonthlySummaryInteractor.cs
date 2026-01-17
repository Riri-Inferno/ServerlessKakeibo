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

            var (totalIncome, totalExpense, expenseByCategory) =
                await _transactionRepository.GetMonthlySummaryAsync(userId, year, month, cancellationToken);

            var balance = totalIncome - totalExpense;

            // 支出トップ3を取得
            var topExpenseCategories = expenseByCategory
                .OrderByDescending(x => x.Value.Amount)
                .Take(3)
                .Select(x => new CategorySummary
                {
                    Category = x.Key,
                    CategoryName = x.Key.ToJapanese(),
                    Amount = x.Value.Amount,
                    Count = x.Value.Count
                })
                .ToList();

            var incomeCount = expenseByCategory.Values.Sum(x => x.Count);
            var expenseCount = expenseByCategory.Values.Sum(x => x.Count);
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
