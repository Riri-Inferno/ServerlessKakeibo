using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;

namespace ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;

/// <summary>
/// 取引カテゴリ復元ユースケース
/// </summary>
public interface IRestoreTransactionCategoryUseCase
{
    Task<TransactionCategoryResult> ExecuteAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken cancellationToken = default);
}