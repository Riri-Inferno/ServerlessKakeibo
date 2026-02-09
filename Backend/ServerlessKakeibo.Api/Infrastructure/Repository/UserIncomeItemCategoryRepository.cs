using Microsoft.EntityFrameworkCore;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Infrastructure.Repository;

/// <summary>
/// ユーザー給与項目カテゴリリポジトリ実装
/// </summary>
public class UserIncomeItemCategoryRepository : IUserIncomeItemCategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ICategoryMasterRepository _masterRepository;

    public UserIncomeItemCategoryRepository(
        ApplicationDbContext context,
        ICategoryMasterRepository masterRepository)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _masterRepository = masterRepository ?? throw new ArgumentNullException(nameof(masterRepository));
    }

    public async Task<List<UserIncomeItemCategoryEntity>> GetByUserSettingsIdAsync(
        Guid userSettingsId,
        bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.UserIncomeItemCategories
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

    public async Task<UserIncomeItemCategoryEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserIncomeItemCategories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task ResetToMasterAsync(
        Guid userSettingsId,
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var masterDerivedCategories = await _context.UserIncomeItemCategories
            .Where(c => c.UserSettingsId == userSettingsId && !c.IsCustom)
            .ToListAsync(cancellationToken);

        _context.UserIncomeItemCategories.RemoveRange(masterDerivedCategories);

        var masters = await _masterRepository.GetAllIncomeItemCategoryMastersAsync(cancellationToken);
        var now = DateTimeOffset.UtcNow;

        var newCategories = masters.Select(master => new UserIncomeItemCategoryEntity
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

        await _context.UserIncomeItemCategories.AddRangeAsync(newCategories, cancellationToken);
    }

    public async Task<List<UserIncomeItemCategoryEntity>> GetByIdsAsync(
    List<Guid> ids,
    CancellationToken cancellationToken = default)
    {
        return await _context.UserIncomeItemCategories
            .Where(c => ids.Contains(c.Id))
            .ToListAsync(cancellationToken);
    }
}
