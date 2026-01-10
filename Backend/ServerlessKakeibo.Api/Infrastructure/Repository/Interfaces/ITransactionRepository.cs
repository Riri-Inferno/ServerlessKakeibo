using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

public interface ITransactionRepository
{
    /// <summary>
    /// 取引詳細を取得(関連データを含む)
    /// </summary>
    Task<TransactionEntity?> GetDetailByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken ct = default);
}
