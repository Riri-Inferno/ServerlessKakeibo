using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;

/// <summary>
/// 取引カテゴリ作成ユースケース
/// </summary>
public interface ICreateTransactionCategoryUseCase
{
    Task<TransactionCategoryResult> ExecuteAsync(
        Guid userId,
        CreateTransactionCategoryRequest request,
        CancellationToken cancellationToken = default);
}
