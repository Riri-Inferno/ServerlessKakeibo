using ServerlessKakeibo.Api.Application.TransactionQuery.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Service.Interface;

namespace ServerlessKakeibo.Api.Application.TransactionQuery;

/// <summary>
/// レシート画像URL取得インタラクター
/// </summary>
public class GetReceiptImageUrlInteractor : IGetReceiptImageUrlUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IGcpStorageService _storageService;
    private readonly ILogger<GetReceiptImageUrlInteractor> _logger;

    public GetReceiptImageUrlInteractor(
        ITransactionRepository transactionRepository,
        IGcpStorageService storageService,
        ILogger<GetReceiptImageUrlInteractor> logger)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// レシート画像の署名付きURLを取得
    /// </summary>
    public async Task<ReceiptImageUrlResult?> ExecuteAsync(
        Guid transactionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (transactionId == Guid.Empty)
            throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            _logger.LogInformation(
                "レシート画像URL取得を開始します。TransactionId: {TransactionId}, UserId: {UserId}",
                transactionId, userId);

            // 1. 取引を取得
            var transaction = await _transactionRepository.GetDetailByIdAsync(
                transactionId, userId, cancellationToken);

            if (transaction == null)
            {
                _logger.LogWarning(
                    "取引が見つかりませんでした。TransactionId: {TransactionId}",
                    transactionId);
                return null;
            }

            // 2. 画像が添付されているか確認
            if (string.IsNullOrEmpty(transaction.SourceUrl))
            {
                _logger.LogWarning(
                    "画像が添付されていません。TransactionId: {TransactionId}",
                    transactionId);
                return null;
            }

            // 3. 署名付きURL生成（1時間有効）
            var signedUrl = await _storageService.GenerateSignedUrlAsync(
                transaction.SourceUrl,
                TimeSpan.FromHours(1),
                cancellationToken);

            _logger.LogInformation(
                "署名付きURLを生成しました。TransactionId: {TransactionId}",
                transactionId);

            return new ReceiptImageUrlResult
            {
                SignedUrl = signedUrl,
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "レシート画像URL取得中にエラーが発生しました。TransactionId: {TransactionId}",
                transactionId);
            throw;
        }
    }
}
