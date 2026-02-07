using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Service.Interface;

namespace ServerlessKakeibo.Api.Service;

/// <summary>
/// カテゴリ初期化サービス実装
/// </summary>
public class CategoryInitializationService : ICategoryInitializationService
{
    private readonly ICategoryMasterRepository _masterRepository;
    private readonly IGenericWriteRepository<UserTransactionCategoryEntity> _transactionCategoryWriteRepository;
    private readonly IGenericWriteRepository<UserItemCategoryEntity> _itemCategoryWriteRepository;
    private readonly IGenericWriteRepository<UserIncomeItemCategoryEntity> _incomeItemCategoryWriteRepository;
    private readonly ILogger<CategoryInitializationService> _logger;

    public CategoryInitializationService(
        ICategoryMasterRepository masterRepository,
        IGenericWriteRepository<UserTransactionCategoryEntity> transactionCategoryWriteRepository,
        IGenericWriteRepository<UserItemCategoryEntity> itemCategoryWriteRepository,
        IGenericWriteRepository<UserIncomeItemCategoryEntity> incomeItemCategoryWriteRepository,
        ILogger<CategoryInitializationService> logger)
    {
        _masterRepository = masterRepository ?? throw new ArgumentNullException(nameof(masterRepository));
        _transactionCategoryWriteRepository = transactionCategoryWriteRepository ?? throw new ArgumentNullException(nameof(transactionCategoryWriteRepository));
        _itemCategoryWriteRepository = itemCategoryWriteRepository ?? throw new ArgumentNullException(nameof(itemCategoryWriteRepository));
        _incomeItemCategoryWriteRepository = incomeItemCategoryWriteRepository ?? throw new ArgumentNullException(nameof(incomeItemCategoryWriteRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// ユーザー設定に対してマスタカテゴリを全件コピーする
    /// </summary>
    public async Task InitializeUserCategoriesAsync(
        Guid userSettingsId,
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "カテゴリ初期化を開始します。UserSettingsId: {UserSettingsId}",
                userSettingsId);

            var now = DateTimeOffset.UtcNow;

            // 1. 取引カテゴリをコピー
            await CopyTransactionCategoriesAsync(
                userSettingsId, userId, tenantId, now, cancellationToken);

            // 2. 商品カテゴリをコピー（支出用）
            await CopyItemCategoriesAsync(
                userSettingsId, userId, tenantId, now, cancellationToken);

            // 3. 給与項目カテゴリをコピー（収入用）
            await CopyIncomeItemCategoriesAsync(
                userSettingsId, userId, tenantId, now, cancellationToken);

            _logger.LogInformation(
                "カテゴリ初期化が完了しました。UserSettingsId: {UserSettingsId}",
                userSettingsId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "カテゴリ初期化中にエラーが発生しました。UserSettingsId: {UserSettingsId}",
                userSettingsId);
            throw;
        }
    }

    /// <summary>
    /// 取引カテゴリをマスタからコピー
    /// </summary>
    private async Task CopyTransactionCategoriesAsync(
        Guid userSettingsId,
        Guid userId,
        Guid tenantId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var masters = await _masterRepository.GetAllTransactionCategoryMastersAsync(cancellationToken);

        _logger.LogDebug(
            "取引カテゴリマスタを {Count} 件取得しました",
            masters.Count);

        var userCategories = masters.Select(master => new UserTransactionCategoryEntity
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

        await _transactionCategoryWriteRepository.AddRangeAsync(
            userCategories, cancellationToken);

        _logger.LogDebug(
            "取引カテゴリを {Count} 件コピーしました",
            userCategories.Count);
    }

    /// <summary>
    /// 商品カテゴリをマスタからコピー（支出用）
    /// </summary>
    private async Task CopyItemCategoriesAsync(
        Guid userSettingsId,
        Guid userId,
        Guid tenantId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var masters = await _masterRepository.GetAllItemCategoryMastersAsync(cancellationToken);

        _logger.LogDebug(
            "商品カテゴリマスタを {Count} 件取得しました",
            masters.Count);

        var userCategories = masters.Select(master => new UserItemCategoryEntity
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

        await _itemCategoryWriteRepository.AddRangeAsync(
            userCategories, cancellationToken);

        _logger.LogDebug(
            "商品カテゴリを {Count} 件コピーしました",
            userCategories.Count);
    }

    /// <summary>
    /// 給与項目カテゴリをマスタからコピー（収入用）
    /// </summary>
    private async Task CopyIncomeItemCategoriesAsync(
        Guid userSettingsId,
        Guid userId,
        Guid tenantId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var masters = await _masterRepository.GetAllIncomeItemCategoryMastersAsync(cancellationToken);

        _logger.LogDebug(
            "給与項目カテゴリマスタを {Count} 件取得しました",
            masters.Count);

        var userCategories = masters.Select(master => new UserIncomeItemCategoryEntity
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

        await _incomeItemCategoryWriteRepository.AddRangeAsync(
            userCategories, cancellationToken);

        _logger.LogDebug(
            "給与項目カテゴリを {Count} 件コピーしました",
            userCategories.Count);
    }
}
