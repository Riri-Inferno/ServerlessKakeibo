using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;

namespace ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;

/// <summary>
/// 取引カテゴリ削除（非表示化）ユースケース
/// </summary>
public interface IDeleteTransactionCategoryUseCase
{
    Task<TransactionCategoryResult> ExecuteAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken cancellationToken = default);
}
