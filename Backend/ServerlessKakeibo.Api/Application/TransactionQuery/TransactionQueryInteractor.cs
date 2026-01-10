using ServerlessKakeibo.Api.Application.TransactionQuery.Dto;
using ServerlessKakeibo.Api.Application.TransactionQuery.Mappers;
using ServerlessKakeibo.Api.Application.TransactionQuery.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.TransactionQuery;

/// <summary>
/// 取引クエリインタラクター
/// </summary>
public class TransactionQueryInteractor : ITransactionQueryUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ILogger<TransactionQueryInteractor> _logger;

    public TransactionQueryInteractor(
        ITransactionRepository transactionRepository,
        ILogger<TransactionQueryInteractor> logger)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 取引詳細を取得
    /// </summary>
    public async Task<TransactionDetailResult?> GetByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Transaction ID cannot be empty", nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            _logger.LogInformation(
                "取引詳細取得を開始します。TransactionId: {TransactionId}, UserId: {UserId}",
                id, userId);

            var entity = await _transactionRepository.GetDetailByIdAsync(id, userId, cancellationToken);

            if (entity == null)
            {
                _logger.LogWarning(
                    "取引が見つかりませんでした。TransactionId: {TransactionId}, UserId: {UserId}",
                    id, userId);
                return null;
            }

            _logger.LogInformation(
                "取引詳細を取得しました。TransactionId: {TransactionId}, Payee: {Payee}, Amount: {Amount}",
                entity.Id, entity.Payee, entity.AmountTotal);

            return TransactionQueryMapper.ToDetailResult(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "取引詳細取得中にエラーが発生しました。TransactionId: {TransactionId}, UserId: {UserId}",
                id, userId);
            throw;
        }
    }
}
