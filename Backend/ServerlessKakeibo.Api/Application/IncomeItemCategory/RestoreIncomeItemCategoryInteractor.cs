using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;
using ServerlessKakeibo.Api.Application.IncomeItemCategory.Mappers;
using ServerlessKakeibo.Api.Application.IncomeItemCategory.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.IncomeItemCategory;

public class RestoreIncomeItemCategoryInteractor : IRestoreIncomeItemCategoryUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IUserIncomeItemCategoryRepository _categoryRepository;
    private readonly IGenericWriteRepository<UserIncomeItemCategoryEntity> _categoryWriteRepository;
    private readonly ILogger<RestoreIncomeItemCategoryInteractor> _logger;

    public RestoreIncomeItemCategoryInteractor(
        ITransactionHelper transactionHelper,
        IUserSettingsRepository userSettingsRepository,
        IUserIncomeItemCategoryRepository categoryRepository,
        IGenericWriteRepository<UserIncomeItemCategoryEntity> categoryWriteRepository,
        ILogger<RestoreIncomeItemCategoryInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _categoryWriteRepository = categoryWriteRepository ?? throw new ArgumentNullException(nameof(categoryWriteRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IncomeItemCategoryResult> ExecuteAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("給与項目カテゴリ復元を開始します。UserId: {UserId}, CategoryId: {CategoryId}", userId, categoryId);

        return await _transactionHelper.ExecuteInTransactionAsync(async () =>
        {
            var userSettings = await _userSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userSettings == null)
            {
                throw new InvalidOperationException("ユーザー設定が見つかりません");
            }

            var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category == null)
            {
                throw new KeyNotFoundException("カテゴリが見つかりません");
            }

            if (category.UserSettingsId != userSettings.Id)
            {
                throw new UnauthorizedAccessException("このカテゴリの復元権限がありません");
            }

            category.IsHidden = false;
            category.UpdatedAt = DateTimeOffset.UtcNow;
            category.UpdatedBy = userId;

            await _categoryWriteRepository.UpdateAsync(category, cancellationToken);

            _logger.LogInformation("給与項目カテゴリを復元しました。CategoryId: {CategoryId}", categoryId);

            return new IncomeItemCategoryResult
            {
                Category = IncomeItemCategoryMapper.ToDto(category),
                Message = "カテゴリを復元しました"
            };
        });
    }
}
