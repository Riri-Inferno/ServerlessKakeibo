using ServerlessKakeibo.Api.Application.Transaction.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.TransactionUpdate.Usecases;

/// <summary>
/// 取引更新ユースケース
/// </summary>
public interface ITransactionUpdateUseCase
{
    /// <summary>
    /// 取引を更新
    /// </summary>
    Task<TransactionResult> ExecuteAsync(
        Guid transactionId,
        UpdateTransactionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);
}
