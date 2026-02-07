using ServerlessKakeibo.Api.Application.ItemCategory.Dto;
using ServerlessKakeibo.Api.Application.ItemCategory.Mappers;
using ServerlessKakeibo.Api.Application.ItemCategory.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.ItemCategory;

public class GetItemCategoriesInteractor : IGetItemCategoriesUseCase
{
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IUserItemCategoryRepository _categoryRepository;
    private readonly ILogger<GetItemCategoriesInteractor> _logger;

    public GetItemCategoriesInteractor(
        IUserSettingsRepository userSettingsRepository,
        IUserItemCategoryRepository categoryRepository,
        ILogger<GetItemCategoriesInteractor> logger)
    {
        _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ItemCategoryListResult> ExecuteAsync(
        Guid userId,
        bool includeHidden,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "商品カテゴリ一覧取得を開始します。UserId: {UserId}, IncludeHidden: {IncludeHidden}",
            userId, includeHidden);

        var userSettings = await _userSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
        if (userSettings == null)
        {
            throw new InvalidOperationException("ユーザー設定が見つかりません");
        }

        var categories = await _categoryRepository.GetByUserSettingsIdAsync(
            userSettings.Id, includeHidden, cancellationToken);

        var dtos = ItemCategoryMapper.ToDtoList(categories);

        _logger.LogInformation("商品カテゴリを {Count} 件取得しました", dtos.Count);

        return new ItemCategoryListResult
        {
            Categories = dtos,
            TotalCount = dtos.Count
        };
    }
}
