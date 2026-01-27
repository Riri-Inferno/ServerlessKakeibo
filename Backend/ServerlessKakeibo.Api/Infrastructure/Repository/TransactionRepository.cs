using Microsoft.EntityFrameworkCore;
using ServerlessKakeibo.Api.Common.Helpers;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Infrastructure.Repository;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 締め日を取得
    /// </summary>
    private async Task<int?> GetClosingDayAsync(Guid userId, CancellationToken ct)
    {
        var settings = await _context.UserSettings
            .AsNoTracking()
            .Where(s => s.UserId == userId && !s.IsDeleted)
            .Select(s => s.ClosingDay)
            .FirstOrDefaultAsync(ct);

        return settings;
    }

    /// <summary>
    /// 取引詳細を取得(関連データを含む)
    /// </summary>
    public async Task<TransactionEntity?> GetDetailByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken ct = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Items)        // ← Eager Loading
            .Include(t => t.Taxes)
            .Include(t => t.ShopDetail)
            .Where(t => t.Id == id
                && t.UserId == userId
                && !t.IsDeleted)
            .FirstOrDefaultAsync(ct);
    }
    /// <summary>
    /// 取引一覧を取得(ページング対応)
    /// </summary>
    public async Task<(List<TransactionEntity> Items, int TotalCount)> GetPagedListAsync(
    Guid userId,
    int page,
    int pageSize,
    DateTimeOffset? startDate = null,
    DateTimeOffset? endDate = null,
    TransactionCategory? category = null,
    string? payee = null,
    decimal? minAmount = null,
    decimal? maxAmount = null,
    TransactionType? type = null,
    CancellationToken ct = default)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId && !t.IsDeleted);

        // フィルタ適用
        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        if (category.HasValue)
            query = query.Where(t => t.Category == category.Value);

        if (!string.IsNullOrWhiteSpace(payee))
            query = query.Where(t => t.Payee != null && t.Payee.Contains(payee));

        if (minAmount.HasValue)
            query = query.Where(t => t.AmountTotal >= minAmount.Value);

        if (maxAmount.HasValue)
            query = query.Where(t => t.AmountTotal <= maxAmount.Value);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        // 総件数取得
        var totalCount = await query.CountAsync(ct);

        // ページング + ソート(新しい順)
        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(t => t.Items)  // ← ItemCountのため
            .ToListAsync(ct);

        return (items, totalCount);
    }

    /// <summary>
    /// 取引を更新用に取得(関連データを含む)
    /// </summary>
    public async Task<TransactionEntity?> GetForUpdateAsync(
        Guid id,
        Guid userId,
        CancellationToken ct = default)
    {
        return await _context.Transactions
            .Include(t => t.Items)
            .Include(t => t.Taxes)
            .Include(t => t.ShopDetail)
            .Where(t => t.Id == id
                && t.UserId == userId
                && !t.IsDeleted)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// 取引に関連する全データを一括で論理削除
    /// </summary>
    /// <param name="transactionId"></param>
    /// <param name="userId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task SoftDeleteWithRelatedDataAsync(
        Guid transactionId,
        Guid userId,
        CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        // 1. 取引項目を一括削除
        await _context.TransactionItems
            .Where(ti => ti.TransactionId == transactionId && !ti.IsDeleted)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(ti => ti.IsDeleted, true)
                    .SetProperty(ti => ti.UpdatedAt, now)
                    .SetProperty(ti => ti.UpdatedBy, userId),
                ct);

        // 2. 税情報を一括削除
        await _context.TaxDetails
            .Where(td => td.TransactionId == transactionId && !td.IsDeleted)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(td => td.IsDeleted, true)
                    .SetProperty(td => td.UpdatedAt, now)
                    .SetProperty(td => td.UpdatedBy, userId),
                ct);

        // 3. 店舗情報を削除
        await _context.ShopDetails
            .Where(sd => sd.TransactionId == transactionId && !sd.IsDeleted)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(sd => sd.IsDeleted, true)
                    .SetProperty(sd => sd.UpdatedAt, now)
                    .SetProperty(sd => sd.UpdatedBy, userId),
                ct);

        // 4. 取引本体を削除
        await _context.Transactions
            .Where(t => t.Id == transactionId && t.UserId == userId && !t.IsDeleted)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(t => t.IsDeleted, true)
                    .SetProperty(t => t.UpdatedAt, now)
                    .SetProperty(t => t.UpdatedBy, userId),
                ct);
    }

    /// <summary>
    /// 月次サマリーを取得
    /// </summary>
    public async Task<(decimal TotalIncome, decimal TotalExpense, Dictionary<TransactionCategory, (decimal Amount, int Count)> ExpenseByCategory)>
        GetMonthlySummaryAsync(
            Guid userId,
            int year,
            int month,
            CancellationToken ct = default)
    {
        // 締め日を考慮した期間取得
        var closingDay = await GetClosingDayAsync(userId, ct);
        var (startDate, endDate) = ClosingDayHelper.GetPeriod(year, month, closingDay);

        var transactions = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId
                && !t.IsDeleted
                && t.TransactionDate >= startDate
                && t.TransactionDate <= endDate)
            .Select(t => new
            {
                t.Type,
                t.AmountTotal,
                t.Category
            })
            .ToListAsync(ct);

        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.AmountTotal ?? 0);

        var totalExpense = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.AmountTotal ?? 0);

        var expenseByCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .ToDictionary(
                g => g.Key,
                g => (Amount: g.Sum(t => t.AmountTotal ?? 0), Count: g.Count())
            );

        return (totalIncome, totalExpense, expenseByCategory);
    }

    /// <summary>
    /// エクスポート用に取引一覧を取得（ページング無視、全件取得）
    /// </summary>
    public async Task<List<TransactionEntity>> GetAllForExportAsync(
        Guid userId,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        TransactionCategory? category = null,
        string? payee = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        TransactionType? type = null,
        CancellationToken ct = default)
    {
        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.Items)        // CSV出力用
            .Include(t => t.Taxes)        // CSV出力用
            .Include(t => t.ShopDetail)   // CSV出力用
            .Where(t => t.UserId == userId && !t.IsDeleted);

        // フィルタ適用（GetPagedListAsyncと同じロジック）
        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        if (category.HasValue)
            query = query.Where(t => t.Category == category.Value);

        if (!string.IsNullOrWhiteSpace(payee))
            query = query.Where(t => t.Payee != null && t.Payee.Contains(payee));

        if (minAmount.HasValue)
            query = query.Where(t => t.AmountTotal >= minAmount.Value);

        if (maxAmount.HasValue)
            query = query.Where(t => t.AmountTotal <= maxAmount.Value);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        // 取引日時の降順でソート
        return await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
    }

    /// <summary>
    /// 全カテゴリの支出内訳を取得（TopN制限なし）
    /// </summary>
    public async Task<Dictionary<TransactionCategory, (decimal Amount, int Count)>> GetAllCategoryExpensesAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken ct = default)
    {
        var closingDay = await GetClosingDayAsync(userId, ct);
        var (startDate, endDate) = ClosingDayHelper.GetPeriod(year, month, closingDay);

        var expenseByCategory = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId
                && !t.IsDeleted
                && t.Type == TransactionType.Expense
                && t.TransactionDate >= startDate
                && t.TransactionDate <= endDate)
            .GroupBy(t => t.Category)
            .Select(g => new
            {
                Category = g.Key,
                Amount = g.Sum(t => t.AmountTotal ?? 0),
                Count = g.Count()
            })
            .ToListAsync(ct);

        return expenseByCategory.ToDictionary(
            x => x.Category,
            x => (x.Amount, x.Count)
        );
    }

    /// <summary>
    /// 指定月の最高額支出取引を取得
    /// </summary>
    public async Task<TransactionEntity?> GetMaxExpenseTransactionAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken ct = default)
    {
        var closingDay = await GetClosingDayAsync(userId, ct);
        var (startDate, endDate) = ClosingDayHelper.GetPeriod(year, month, closingDay);

        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId
                && !t.IsDeleted
                && t.Type == TransactionType.Expense
                && t.TransactionDate >= startDate
                && t.TransactionDate <= endDate)
            .OrderByDescending(t => t.AmountTotal)
            .FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// 指定月の最も頻度の高いカテゴリを取得
    /// </summary>
    public async Task<(TransactionCategory Category, int Count, decimal TotalAmount)?> GetMostFrequentCategoryAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken ct = default)
    {
        var closingDay = await GetClosingDayAsync(userId, ct);
        var (startDate, endDate) = ClosingDayHelper.GetPeriod(year, month, closingDay);

        var result = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId
                && !t.IsDeleted
                && t.Type == TransactionType.Expense
                && t.TransactionDate >= startDate
                && t.TransactionDate <= endDate)
            .GroupBy(t => t.Category)
            .Select(g => new
            {
                Category = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(t => t.AmountTotal ?? 0)
            })
            .OrderByDescending(x => x.Count)
            .ThenByDescending(x => x.TotalAmount)
            .FirstOrDefaultAsync(ct);

        if (result == null)
            return null;

        return (result.Category, result.Count, result.TotalAmount);
    }

    /// <summary>
    /// 指定月の支出があった日数を取得
    /// </summary>
    public async Task<int> GetDaysWithExpenseAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken ct = default)
    {
        var closingDay = await GetClosingDayAsync(userId, ct);
        var (startDate, endDate) = ClosingDayHelper.GetPeriod(year, month, closingDay);

        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId
                && !t.IsDeleted
                && t.Type == TransactionType.Expense
                && t.TransactionDate >= startDate
                && t.TransactionDate <= endDate)
            .Select(t => t.TransactionDate!.Value.Date)
            .Distinct()
            .CountAsync(ct);
    }

    /// <summary>
    /// 複数月の月次サマリーを一括取得（月次推移用）
    /// </summary>
    public async Task<List<(int Year, int Month, decimal Income, decimal Expense, decimal Balance)>> GetMonthlyAggregatesAsync(
        Guid userId,
        List<(int Year, int Month)> monthlyRanges,
        CancellationToken ct = default)
    {
        // 締め日を一度だけ取得
        var closingDay = await GetClosingDayAsync(userId, ct);
        var results = new List<(int Year, int Month, decimal Income, decimal Expense, decimal Balance)>();

        foreach (var (year, month) in monthlyRanges)
        {
            var (startDate, endDate) = ClosingDayHelper.GetPeriod(year, month, closingDay);

            var transactions = await _context.Transactions
                .AsNoTracking()
                .Where(t => t.UserId == userId
                    && !t.IsDeleted
                    && t.TransactionDate >= startDate
                    && t.TransactionDate <= endDate)
                .Select(t => new
                {
                    t.Type,
                    t.AmountTotal
                })
                .ToListAsync(ct);

            var income = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.AmountTotal ?? 0);

            var expense = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.AmountTotal ?? 0);

            var balance = income - expense;

            results.Add((year, month, income, expense, balance));
        }

        return results;
    }
}
