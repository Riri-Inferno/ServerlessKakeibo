using ServerlessKakeibo.Api.Application.TransactionQuery.Dto;
using ServerlessKakeibo.Api.Application.TransactionQuery.Mappers;
using ServerlessKakeibo.Api.Application.TransactionQuery.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Common.Models;
using ServerlessKakeibo.Api.Contracts;

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

    /// <summary>
    /// 取引一覧を取得
    /// </summary>
    public async Task<PagedResult<TransactionSummaryResult>> GetPagedListAsync(
        GetTransactionsRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            _logger.LogInformation(
                "取引一覧取得を開始します。UserId: {UserId}, Page: {Page}, PageSize: {PageSize}",
                userId, request.Page, request.PageSize);

            var (items, totalCount) = await _transactionRepository.GetPagedListAsync(
                userId,
                request.Page,
                request.PageSize,
                request.StartDate,
                request.EndDate,
                request.Category,
                request.Payer,
                request.Payee,
                request.MinAmount,
                request.MaxAmount,
                request.Type,
                cancellationToken);

            _logger.LogInformation(
                "取引一覧を取得しました。UserId: {UserId}, 取得件数: {Count}, 総件数: {TotalCount}",
                userId, items.Count, totalCount);

            return new PagedResult<TransactionSummaryResult>
            {
                Items = items.Select(TransactionQueryMapper.ToSummaryResult).ToList(),
                CurrentPage = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取引一覧取得中にエラーが発生しました。UserId: {UserId}", userId);
            throw;
        }
    }
}
