using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;
using ServerlessKakeibo.Api.Application.TransactionCategory.Mappers;
using ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.TransactionCategory;

/// <summary>
/// 取引カテゴリ更新インタラクター
/// </summary>
public class UpdateTransactionCategoryInteractor : IUpdateTransactionCategoryUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IUserTransactionCategoryRepository _categoryRepository;
    private readonly IGenericWriteRepository<UserTransactionCategoryEntity> _categoryWriteRepository;
    private readonly ILogger<UpdateTransactionCategoryInteractor> _logger;

    public UpdateTransactionCategoryInteractor(
        ITransactionHelper transactionHelper,
        IUserSettingsRepository userSettingsRepository,
        IUserTransactionCategoryRepository categoryRepository,
        IGenericWriteRepository<UserTransactionCategoryEntity> categoryWriteRepository,
        ILogger<UpdateTransactionCategoryInteractor> logger)
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
        UpdateTransactionCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "取引カテゴリ更新を開始します。UserId: {UserId}, CategoryId: {CategoryId}",
            userId, categoryId);

        return await _transactionHelper.ExecuteInTransactionAsync(async () =>
        {
            // ユーザー設定を取得
            var userSettings = await _userSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userSettings == null)
            {
                throw new InvalidOperationException("ユーザー設定が見つかりません");
            }

            // カテゴリを取得
            var category = await _categoryRepository.GetByIdAsync(categoryId, cancellationToken);
            if (category == null)
            {
                throw new KeyNotFoundException("カテゴリが見つかりません");
            }

            // 所有権チェック
            if (category.UserSettingsId != userSettings.Id)
            {
                throw new UnauthorizedAccessException("このカテゴリの編集権限がありません");
            }

            // 更新
            category.Name = request.Name;
            category.ColorCode = request.ColorCode;
            category.DisplayOrder = request.DisplayOrder;
            category.UpdatedAt = DateTimeOffset.UtcNow;
            category.UpdatedBy = userId;

            await _categoryWriteRepository.UpdateAsync(category, cancellationToken);

            _logger.LogInformation(
                "取引カテゴリを更新しました。CategoryId: {CategoryId}",
                categoryId);

            return new TransactionCategoryResult
            {
                Category = TransactionCategoryMapper.ToDto(category),
                Message = "カテゴリを更新しました"
            };
        });
    }
}
