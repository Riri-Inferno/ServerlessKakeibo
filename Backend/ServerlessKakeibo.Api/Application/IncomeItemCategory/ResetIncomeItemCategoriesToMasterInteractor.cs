using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;
using ServerlessKakeibo.Api.Application.IncomeItemCategory.Mappers;
using ServerlessKakeibo.Api.Application.IncomeItemCategory.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.IncomeItemCategory;

public class ResetIncomeItemCategoriesToMasterInteractor : IResetIncomeItemCategoriesToMasterUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IUserIncomeItemCategoryRepository _categoryRepository;
    private readonly ILogger<ResetIncomeItemCategoriesToMasterInteractor> _logger;

    public ResetIncomeItemCategoriesToMasterInteractor(
        ITransactionHelper transactionHelper,
        IUserSettingsRepository userSettingsRepository,
        IUserIncomeItemCategoryRepository categoryRepository,
        ILogger<ResetIncomeItemCategoriesToMasterInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IncomeItemCategoryListResult> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("給与項目カテゴリのマスタ設定リセットを開始します。UserId: {UserId}", userId);

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

            var dtos = IncomeItemCategoryMapper.ToDtoList(categories);

            _logger.LogInformation("給与項目カテゴリをマスタ設定にリセットしました。復元件数: {Count}", dtos.Count);

            return new IncomeItemCategoryListResult
            {
                Categories = dtos,
                TotalCount = dtos.Count
            };
        });
    }
}
