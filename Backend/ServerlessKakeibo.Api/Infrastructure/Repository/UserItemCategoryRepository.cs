using Microsoft.EntityFrameworkCore;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Infrastructure.Repository;

/// <summary>
/// ユーザー商品カテゴリリポジトリ実装
/// </summary>
public class UserItemCategoryRepository : IUserItemCategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICategoryMasterRepository _masterRepository;

    public UserItemCategoryRepository(
        ApplicationDbContext context,
        ICategoryMasterRepository masterRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _masterRepository = masterRepository ?? throw new ArgumentNullException(nameof(masterRepository));
    }

    public async Task<List<UserItemCategoryEntity>> GetByUserSettingsIdAsync(
        Guid userSettingsId,
        bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UserItemCategories
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

    public async Task<UserItemCategoryEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserItemCategories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task ResetToMasterAsync(
        Guid userSettingsId,
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var masterDerivedCategories = await _context.UserItemCategories
            .Where(c => c.UserSettingsId == userSettingsId && !c.IsCustom)
            .ToListAsync(cancellationToken);

        _context.UserItemCategories.RemoveRange(masterDerivedCategories);

        var masters = await _masterRepository.GetAllItemCategoryMastersAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var newCategories = masters.Select(master => new UserItemCategoryEntity
        {
            Id = Guid.NewGuid(),
            UserSettingsId = userSettingsId,
            MasterCategoryId = master.Id,
            Name = master.Name,
            Code = master.Code,
            ColorCode = master.ColorCode,
            DisplayOrder = master.DisplayOrder,
            IsCustom = false,
            IsHidden = false,
            TenantId = tenantId,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = userId,
            UpdatedBy = userId
        }).ToList();

        await _context.UserItemCategories.AddRangeAsync(newCategories, cancellationToken);
    }

    public async Task<List<UserItemCategoryEntity>> GetByIdsAsync(
    List<Guid> ids,
    CancellationToken cancellationToken = default)
    {
        return await _context.UserItemCategories
            .Where(c => ids.Contains(c.Id))
            .ToListAsync(cancellationToken);
    }
}
