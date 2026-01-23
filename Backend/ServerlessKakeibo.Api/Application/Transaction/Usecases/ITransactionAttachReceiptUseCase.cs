using ServerlessKakeibo.Api.Application.Transaction.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.Transaction.Usecases;

/// <summary>
/// 取引へのレシート画像添付ユースケース
/// </summary>
public interface ITransactionAttachReceiptUseCase
{
    /// <summary>
    /// 既存の取引にレシート画像を添付する
    /// </summary>
    /// <param name="transactionId">取引ID</param>
    /// <param name="request">添付リクエスト</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>更新後の取引情報</returns>
    Task<TransactionResult> ExecuteAsync(
        Guid transactionId,
        AttachReceiptRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);
}
