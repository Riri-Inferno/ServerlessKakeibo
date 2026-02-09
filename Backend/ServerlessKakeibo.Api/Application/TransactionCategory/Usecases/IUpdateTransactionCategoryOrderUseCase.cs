using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;

/// <summary>
/// 取引カテゴリ並び順一括更新ユースケース
/// </summary>
public interface IUpdateTransactionCategoryOrderUseCase
{
    Task<TransactionCategoryListResult> ExecuteAsync(
        Guid userId,
        UpdateTransactionCategoryOrderRequest request,
        CancellationToken cancellationToken = default);
}
