using Microsoft.EntityFrameworkCore;
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
}
