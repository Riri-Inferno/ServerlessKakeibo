using Microsoft.EntityFrameworkCore;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Infrastructure.Repository;

/// <summary>
/// カテゴリマスタリポジトリ実装
/// </summary>
public class CategoryMasterRepository : ICategoryMasterRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryMasterRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// 取引カテゴリマスタを全件取得
    /// </summary>
    public async Task<List<TransactionCategoryMasterEntity>> GetAllTransactionCategoryMastersAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.TransactionCategoryMasters
            .AsNoTracking()
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 商品カテゴリマスタを全件取得
    /// </summary>
    public async Task<List<ItemCategoryMasterEntity>> GetAllItemCategoryMastersAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.ItemCategoryMasters
            .AsNoTracking()
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// 給与項目カテゴリマスタを全件取得
    /// </summary>
    public async Task<List<IncomeItemCategoryMasterEntity>> GetAllIncomeItemCategoryMastersAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.IncomeItemCategoryMasters
            .AsNoTracking()
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync(cancellationToken);
    }
}
