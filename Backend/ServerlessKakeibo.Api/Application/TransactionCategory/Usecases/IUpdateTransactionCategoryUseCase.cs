using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;

/// <summary>
/// 取引カテゴリ更新ユースケース
/// </summary>
public interface IUpdateTransactionCategoryUseCase
{
    Task<TransactionCategoryResult> ExecuteAsync(
        Guid userId,
        Guid categoryId,
        UpdateTransactionCategoryRequest request,
        CancellationToken cancellationToken = default);
}
