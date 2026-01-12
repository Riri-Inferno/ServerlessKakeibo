using ServerlessKakeibo.Api.Application.Transaction.Dto;

namespace ServerlessKakeibo.Api.Application.Transaction.Usecases;

/// <summary>
/// 取引削除ユースケース
/// </summary>
public interface ITransactionDeleteUseCase
{
    /// <summary>
    /// 取引を削除
    /// </summary>
    /// <param name="transactionId"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TransactionDeleteResult> ExecuteAsync(
        Guid transactionId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
