using Microsoft.EntityFrameworkCore;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Infrastructure.Repository;

/// <summary>
/// ユーザー取引カテゴリリポジトリ実装
/// </summary>
public class UserTransactionCategoryRepository : IUserTransactionCategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICategoryMasterRepository _masterRepository;

    public UserTransactionCategoryRepository(
        ApplicationDbContext context,
        ICategoryMasterRepository masterRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _masterRepository = masterRepository ?? throw new ArgumentNullException(nameof(masterRepository));
    }

    /// <summary>
    /// ユーザー設定IDでカテゴリ一覧を取得
    /// </summary>
    public async Task<List<UserTransactionCategoryEntity>> GetByUserSettingsIdAsync(
        Guid userSettingsId,
        bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UserTransactionCategories
            .AsNoTracking()
            .Where(c => c.UserSettingsId == userSettingsId);

        if (!includeHidden)
        {
            query = query.Where(c => !c.IsHidden);
        }

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// カテゴリIDでカテゴリを取得
    /// </summary>
    public async Task<UserTransactionCategoryEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserTransactionCategories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// マスタ由来のカテゴリを全削除し、マスタから再コピー
    /// </summary>
    public async Task ResetToMasterAsync(
        Guid userSettingsId,
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        // 1. マスタ由来のカテゴリを削除（IsCustom=falseのもの）
        var masterDerivedCategories = await _context.UserTransactionCategories
            .Where(c => c.UserSettingsId == userSettingsId && !c.IsCustom)
            .ToListAsync(cancellationToken);

        _context.UserTransactionCategories.RemoveRange(masterDerivedCategories);

        // 2. マスタから再コピー
        var masters = await _masterRepository.GetAllTransactionCategoryMastersAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var newCategories = masters.Select(master => new UserTransactionCategoryEntity
        {
            Id = Guid.NewGuid(),
            UserSettingsId = userSettingsId,
            MasterCategoryId = master.Id,
            Name = master.Name,
            Code = master.Code,
            ColorCode = master.ColorCode,
            DisplayOrder = master.DisplayOrder,
            IsIncome = master.IsIncome,
            IsCustom = false,
            IsHidden = false,
            TenantId = tenantId,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = userId,
            UpdatedBy = userId
        }).ToList();

        await _context.UserTransactionCategories.AddRangeAsync(newCategories, cancellationToken);
    }

    /// <summary>
    /// 複数IDでカテゴリを取得
    /// </summary>
    public async Task<List<UserTransactionCategoryEntity>> GetByIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserTransactionCategories
            .Where(c => ids.Contains(c.Id))
            .ToListAsync(cancellationToken);
    }
}
