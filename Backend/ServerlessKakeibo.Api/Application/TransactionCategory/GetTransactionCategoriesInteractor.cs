using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;
using ServerlessKakeibo.Api.Application.TransactionCategory.Mappers;
using ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.TransactionCategory;

/// <summary>
/// 取引カテゴリ一覧取得インタラクター
/// </summary>
public class GetTransactionCategoriesInteractor : IGetTransactionCategoriesUseCase
{
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IUserTransactionCategoryRepository _categoryRepository;
    private readonly ILogger<GetTransactionCategoriesInteractor> _logger;

    public GetTransactionCategoriesInteractor(
        IUserSettingsRepository userSettingsRepository,
        IUserTransactionCategoryRepository categoryRepository,
        ILogger<GetTransactionCategoriesInteractor> logger)
    {
        _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TransactionCategoryListResult> ExecuteAsync(
        Guid userId,
        bool includeHidden,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "取引カテゴリ一覧取得を開始します。UserId: {UserId}, IncludeHidden: {IncludeHidden}",
            userId, includeHidden);

        // ユーザー設定を取得
        var userSettings = await _userSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
        if (userSettings == null)
        {
            throw new InvalidOperationException("ユーザー設定が見つかりません");
        }

        // カテゴリ一覧を取得
        var categories = await _categoryRepository.GetByUserSettingsIdAsync(
            userSettings.Id, includeHidden, cancellationToken);

        var dtos = TransactionCategoryMapper.ToDtoList(categories);

        _logger.LogInformation(
            "取引カテゴリを {Count} 件取得しました",
            dtos.Count);

        return new TransactionCategoryListResult
        {
            Categories = dtos,
            TotalCount = dtos.Count
        };
    }
}
