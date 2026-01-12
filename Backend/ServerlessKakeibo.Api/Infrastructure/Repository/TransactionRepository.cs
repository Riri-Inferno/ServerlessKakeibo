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
}
