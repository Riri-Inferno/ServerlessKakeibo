using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;

namespace ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;

/// <summary>
/// 取引カテゴリをマスタ設定に戻すユースケース
/// </summary>
public interface IResetTransactionCategoriesToMasterUseCase
{
    Task<TransactionCategoryListResult> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
