using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;
using ServerlessKakeibo.Api.Application.TransactionCategory.Mappers;
using ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.TransactionCategory;

/// <summary>
/// 取引カテゴリをマスタ設定に戻すインタラクター
/// </summary>
public class ResetTransactionCategoriesToMasterInteractor : IResetTransactionCategoriesToMasterUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IUserTransactionCategoryRepository _categoryRepository;
    private readonly ILogger<ResetTransactionCategoriesToMasterInteractor> _logger;

    public ResetTransactionCategoriesToMasterInteractor(
        ITransactionHelper transactionHelper,
        IUserSettingsRepository userSettingsRepository,
        IUserTransactionCategoryRepository categoryRepository,
        ILogger<ResetTransactionCategoriesToMasterInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TransactionCategoryListResult> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "取引カテゴリのマスタ設定リセットを開始します。UserId: {UserId}",
            userId);

        return await _transactionHelper.ExecuteInTransactionAsync(async () =>
        {
            var userSettings = await _userSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userSettings == null)
            {
                throw new InvalidOperationException("ユーザー設定が見つかりません");
            }

            // マスタ由来のカテゴリを削除し、マスタから再コピー
            await _categoryRepository.ResetToMasterAsync(
                userSettings.Id,
                userId,
                userSettings.TenantId,
                cancellationToken);

            // 更新後のカテゴリ一覧を取得
            var categories = await _categoryRepository.GetByUserSettingsIdAsync(
                userSettings.Id, false, cancellationToken);

            var dtos = TransactionCategoryMapper.ToDtoList(categories);

            _logger.LogInformation(
                "取引カテゴリをマスタ設定にリセットしました。復元件数: {Count}",
                dtos.Count);

            return new TransactionCategoryListResult
            {
                Categories = dtos,
                TotalCount = dtos.Count
            };
        });
    }
}
