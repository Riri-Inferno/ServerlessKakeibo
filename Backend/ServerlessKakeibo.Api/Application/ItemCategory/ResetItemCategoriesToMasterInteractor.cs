using ServerlessKakeibo.Api.Application.ItemCategory.Dto;
using ServerlessKakeibo.Api.Application.ItemCategory.Mappers;
using ServerlessKakeibo.Api.Application.ItemCategory.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.ItemCategory;

public class ResetItemCategoriesToMasterInteractor : IResetItemCategoriesToMasterUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IUserItemCategoryRepository _categoryRepository;
    private readonly ILogger<ResetItemCategoriesToMasterInteractor> _logger;

    public ResetItemCategoriesToMasterInteractor(
        ITransactionHelper transactionHelper,
        IUserSettingsRepository userSettingsRepository,
        IUserItemCategoryRepository categoryRepository,
        ILogger<ResetItemCategoriesToMasterInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ItemCategoryListResult> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("商品カテゴリのマスタ設定リセットを開始します。UserId: {UserId}", userId);

        return await _transactionHelper.ExecuteInTransactionAsync(async () =>
        {
            var userSettings = await _userSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userSettings == null)
            {
                throw new InvalidOperationException("ユーザー設定が見つかりません");
            }

            await _categoryRepository.ResetToMasterAsync(
                userSettings.Id,
                userId,
                userSettings.TenantId,
                cancellationToken);

            var categories = await _categoryRepository.GetByUserSettingsIdAsync(
                userSettings.Id, false, cancellationToken);

            var dtos = ItemCategoryMapper.ToDtoList(categories);

            _logger.LogInformation("商品カテゴリをマスタ設定にリセットしました。復元件数: {Count}", dtos.Count);

            return new ItemCategoryListResult
            {
                Categories = dtos,
                TotalCount = dtos.Count
            };
        });
    }
}
