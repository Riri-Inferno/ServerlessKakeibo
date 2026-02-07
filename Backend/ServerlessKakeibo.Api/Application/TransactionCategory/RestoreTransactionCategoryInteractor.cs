using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;
using ServerlessKakeibo.Api.Application.TransactionCategory.Mappers;
using ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.TransactionCategory;

/// <summary>
/// 取引カテゴリ復元インタラクター
/// </summary>
public class RestoreTransactionCategoryInteractor : IRestoreTransactionCategoryUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IUserTransactionCategoryRepository _categoryRepository;
    private readonly IGenericWriteRepository<UserTransactionCategoryEntity> _categoryWriteRepository;
    private readonly ILogger<RestoreTransactionCategoryInteractor> _logger;

    public RestoreTransactionCategoryInteractor(
        ITransactionHelper transactionHelper,
        IUserSettingsRepository userSettingsRepository,
        IUserTransactionCategoryRepository categoryRepository,
        IGenericWriteRepository<UserTransactionCategoryEntity> categoryWriteRepository,
        ILogger<RestoreTransactionCategoryInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _categoryWriteRepository = categoryWriteRepository ?? throw new ArgumentNullException(nameof(categoryWriteRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TransactionCategoryResult> ExecuteAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "取引カテゴリ復元を開始します。UserId: {UserId}, CategoryId: {CategoryId}",
            userId, categoryId);

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

            // 復元
            category.IsHidden = false;
            category.UpdatedAt = DateTimeOffset.UtcNow;
            category.UpdatedBy = userId;

            await _categoryWriteRepository.UpdateAsync(category, cancellationToken);

            _logger.LogInformation(
                "取引カテゴリを復元しました。CategoryId: {CategoryId}",
                categoryId);

            return new TransactionCategoryResult
            {
                Category = TransactionCategoryMapper.ToDto(category),
                Message = "カテゴリを復元しました"
            };
        });
    }
}
