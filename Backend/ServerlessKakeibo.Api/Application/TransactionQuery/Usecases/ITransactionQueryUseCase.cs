using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Application.TransactionQuery.Dto;
using ServerlessKakeibo.Api.Common.Models;

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

    /// <summary>
    /// 取引一覧を取得
    /// </summary>
    Task<PagedResult<TransactionSummaryResult>> GetPagedListAsync(
        GetTransactionsRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);
}
