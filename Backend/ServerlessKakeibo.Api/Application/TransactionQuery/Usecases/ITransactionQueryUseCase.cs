using ServerlessKakeibo.Api.Application.TransactionQuery.Dto;

namespace ServerlessKakeibo.Api.Application.TransactionQuery.Usecases;

/// <summary>
/// 取引クエリユースケース
/// </summary>
public interface ITransactionQueryUseCase
{
    /// <summary>
    /// 取引詳細を取得
    /// </summary>
    /// <param name="id">取引ID</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>取引詳細。存在しない場合はnull</returns>
    Task<TransactionDetailResult?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default);
}
