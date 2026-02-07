using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;

namespace ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;

/// <summary>
/// 取引カテゴリ一覧取得ユースケース
/// </summary>
public interface IGetTransactionCategoriesUseCase
{
    Task<TransactionCategoryListResult> ExecuteAsync(
        Guid userId,
        bool includeHidden,
        CancellationToken cancellationToken = default);
}
