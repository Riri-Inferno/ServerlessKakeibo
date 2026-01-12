using ServerlessKakeibo.Api.Application.Transaction.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.Transaction.Usecases;

/// <summary>
/// 取引作成ユースケース
/// </summary>
public interface ITransactionCreateUseCase
{
    /// <summary>
    /// 取引を新規作成
    /// </summary>
    Task<TransactionResult> ExecuteAsync(
        CreateTransactionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);
}
