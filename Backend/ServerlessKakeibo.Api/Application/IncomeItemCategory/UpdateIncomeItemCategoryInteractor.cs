using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;
using ServerlessKakeibo.Api.Application.IncomeItemCategory.Mappers;
using ServerlessKakeibo.Api.Application.IncomeItemCategory.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.IncomeItemCategory;

public class UpdateIncomeItemCategoryInteractor : IUpdateIncomeItemCategoryUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IUserIncomeItemCategoryRepository _categoryRepository;
    private readonly IGenericWriteRepository<UserIncomeItemCategoryEntity> _categoryWriteRepository;
    private readonly ILogger<UpdateIncomeItemCategoryInteractor> _logger;

    public UpdateIncomeItemCategoryInteractor(
        ITransactionHelper transactionHelper,
        IUserSettingsRepository userSettingsRepository,
        IUserIncomeItemCategoryRepository categoryRepository,
        IGenericWriteRepository<UserIncomeItemCategoryEntity> categoryWriteRepository,
        ILogger<UpdateIncomeItemCategoryInteractor> logger)
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
        UpdateIncomeItemCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("給与項目カテゴリ更新を開始します。UserId: {UserId}, CategoryId: {CategoryId}", userId, categoryId);

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
                throw new UnauthorizedAccessException("このカテゴリの編集権限がありません");
            }

            category.Name = request.Name;
            category.ColorCode = request.ColorCode;
            category.DisplayOrder = request.DisplayOrder;
            category.UpdatedAt = DateTimeOffset.UtcNow;
            category.UpdatedBy = userId;

            await _categoryWriteRepository.UpdateAsync(category, cancellationToken);

            _logger.LogInformation("給与項目カテゴリを更新しました。CategoryId: {CategoryId}", categoryId);

            return new IncomeItemCategoryResult
            {
                Category = IncomeItemCategoryMapper.ToDto(category),
                Message = "カテゴリを更新しました"
            };
        });
    }
}
