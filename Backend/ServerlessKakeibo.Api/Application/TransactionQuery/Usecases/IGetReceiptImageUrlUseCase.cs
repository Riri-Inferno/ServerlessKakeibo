using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.TransactionQuery.Usecases;

/// <summary>
/// レシート画像URL取得ユースケース
/// </summary>
public interface IGetReceiptImageUrlUseCase
{
    /// <summary>
    /// レシート画像の署名付きURLを取得
    /// </summary>
    /// <param name="transactionId">取引ID</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>署名付きURL結果（画像が存在しない場合はnull）</returns>
    Task<ReceiptImageUrlResult?> ExecuteAsync(
        Guid transactionId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
