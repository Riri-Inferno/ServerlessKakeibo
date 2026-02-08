using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;
using ServerlessKakeibo.Api.Application.IncomeItemCategory.Mappers;
using ServerlessKakeibo.Api.Application.IncomeItemCategory.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.IncomeItemCategory;

public class GetIncomeItemCategoriesInteractor : IGetIncomeItemCategoriesUseCase
{
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IUserIncomeItemCategoryRepository _categoryRepository;
    private readonly ILogger<GetIncomeItemCategoriesInteractor> _logger;

    public GetIncomeItemCategoriesInteractor(
        IUserSettingsRepository userSettingsRepository,
        IUserIncomeItemCategoryRepository categoryRepository,
        ILogger<GetIncomeItemCategoriesInteractor> logger)
    {
        _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IncomeItemCategoryListResult> ExecuteAsync(
        Guid userId,
        bool includeHidden,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "給与項目カテゴリ一覧取得を開始します。UserId: {UserId}, IncludeHidden: {IncludeHidden}",
            userId, includeHidden);

        var userSettings = await _userSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
        if (userSettings == null)
        {
            throw new InvalidOperationException("ユーザー設定が見つかりません");
        }

        var categories = await _categoryRepository.GetByUserSettingsIdAsync(
            userSettings.Id, includeHidden, cancellationToken);

        var dtos = IncomeItemCategoryMapper.ToDtoList(categories);

        _logger.LogInformation("給与項目カテゴリを {Count} 件取得しました", dtos.Count);

        return new IncomeItemCategoryListResult
        {
            Categories = dtos,
            TotalCount = dtos.Count
        };
    }
}
