using ServerlessKakeibo.Api.Domain.ValueObjects;
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

    /// <summary>
    /// 取引一覧を取得(ページング対応)
    /// </summary>
    Task<(List<TransactionEntity> Items, int TotalCount)> GetPagedListAsync(
        Guid userId,
        int page,
        int pageSize,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        TransactionCategory? category = null,
        string? payee = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        TransactionType? type = null,
        CancellationToken ct = default);

    /// <summary>
    /// 取引を更新用に取得(関連データを含む)
    /// </summary>
    Task<TransactionEntity?> GetForUpdateAsync(
        Guid id,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// 取引に関連する全データを一括で論理削除
    /// </summary>
    Task SoftDeleteWithRelatedDataAsync(
        Guid transactionId,
        Guid userId,
        CancellationToken ct = default);
}
