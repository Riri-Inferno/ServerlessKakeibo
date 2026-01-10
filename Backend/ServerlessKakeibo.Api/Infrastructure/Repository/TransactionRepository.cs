using Microsoft.EntityFrameworkCore;
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
}
