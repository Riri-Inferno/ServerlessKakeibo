using ServerlessKakeibo.Api.Application.ItemCategory.Dto;
using ServerlessKakeibo.Api.Application.ItemCategory.Mappers;
using ServerlessKakeibo.Api.Application.ItemCategory.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.ItemCategory;

/// <summary>
/// 商品カテゴリ並び順一括更新インタラクター
/// </summary>
public class UpdateItemCategoryOrderInteractor : IUpdateItemCategoryOrderUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IUserItemCategoryRepository _categoryRepository;
    private readonly IGenericWriteRepository<UserItemCategoryEntity> _categoryWriteRepository;
    private readonly ILogger<UpdateItemCategoryOrderInteractor> _logger;

    public UpdateItemCategoryOrderInteractor(
        ITransactionHelper transactionHelper,
        IUserSettingsRepository userSettingsRepository,
        IUserItemCategoryRepository categoryRepository,
        IGenericWriteRepository<UserItemCategoryEntity> categoryWriteRepository,
        ILogger<UpdateItemCategoryOrderInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _categoryWriteRepository = categoryWriteRepository ?? throw new ArgumentNullException(nameof(categoryWriteRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ItemCategoryListResult> ExecuteAsync(
        Guid userId,
        UpdateItemCategoryOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "商品カテゴリ並び順一括更新を開始します。UserId: {UserId}, 対象件数: {Count}",
            userId, request.Orders.Count);

        Guid userSettingsId = Guid.Empty;

        await _transactionHelper.ExecuteInTransactionAsync(async () =>
        {
            // ユーザー設定を取得
            var userSettings = await _userSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userSettings == null)
            {
                throw new InvalidOperationException("ユーザー設定が見つかりません");
            }

            userSettingsId = userSettings.Id;

            // 対象カテゴリIDリスト
            var categoryIds = request.Orders.Select(o => o.Id).ToList();

            // カテゴリを一括取得
            var categories = await _categoryRepository.GetByIdsAsync(categoryIds, cancellationToken);

            // 存在チェック
            if (categories.Count != categoryIds.Count)
            {
                var foundIds = categories.Select(c => c.Id).ToHashSet();
                var notFoundIds = categoryIds.Where(id => !foundIds.Contains(id)).ToList();
                throw new KeyNotFoundException(
                    $"以下のカテゴリが見つかりません: {string.Join(", ", notFoundIds)}");
            }

            // 所有権チェック
            var unauthorizedCategories = categories
                .Where(c => c.UserSettingsId != userSettings.Id)
                .ToList();

            if (unauthorizedCategories.Any())
            {
                throw new UnauthorizedAccessException("編集権限がないカテゴリが含まれています");
            }

            // 並び順を更新
            var now = DateTimeOffset.UtcNow;
            foreach (var order in request.Orders)
            {
                var category = categories.First(c => c.Id == order.Id);
                category.DisplayOrder = order.DisplayOrder;
                category.UpdatedAt = now;
                category.UpdatedBy = userId;
            }

            // 一括更新
            await _categoryWriteRepository.UpdateRangeAsync(categories, cancellationToken);

            _logger.LogInformation(
                "商品カテゴリ並び順を更新しました。対象件数: {Count}",
                categories.Count);
        });

        // トランザクションコミット後に再取得
        var allCategories = await _categoryRepository.GetByUserSettingsIdAsync(
            userSettingsId,
            includeHidden: false,
            cancellationToken);

        return new ItemCategoryListResult
        {
            Categories = allCategories
                .OrderBy(c => c.DisplayOrder)
                .Select(ItemCategoryMapper.ToDto)
                .ToList(),
            TotalCount = allCategories.Count
        };
    }
}
