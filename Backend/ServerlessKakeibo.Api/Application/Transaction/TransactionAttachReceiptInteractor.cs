using ServerlessKakeibo.Api.Application.Transaction.Dto;
using ServerlessKakeibo.Api.Application.Transaction.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Domain.Transaction.Policies;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Service.Interface;
using ServerlessKakeibo.Api.Common.Helpers;

namespace ServerlessKakeibo.Api.Application.Transaction;

/// <summary>
/// 取引へのレシート画像添付インタラクター
/// </summary>
public class TransactionAttachReceiptInteractor : ITransactionAttachReceiptUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IGenericReadRepository<TransactionEntity> _transactionReadRepository;
    private readonly IGenericWriteRepository<TransactionEntity> _transactionWriteRepository;
    private readonly IGcpStorageService _storageService;
    private readonly ILogger<TransactionAttachReceiptInteractor> _logger;

    public TransactionAttachReceiptInteractor(
        ITransactionHelper transactionHelper,
        IGenericReadRepository<TransactionEntity> transactionReadRepository,
        IGenericWriteRepository<TransactionEntity> transactionWriteRepository,
        IGcpStorageService storageService,
        ILogger<TransactionAttachReceiptInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _transactionReadRepository = transactionReadRepository ??
            throw new ArgumentNullException(nameof(transactionReadRepository));
        _transactionWriteRepository = transactionWriteRepository ??
            throw new ArgumentNullException(nameof(transactionWriteRepository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 既存の取引にレシート画像を添付する
    /// </summary>
    public async Task<TransactionResult> ExecuteAsync(
        Guid transactionId,
        AttachReceiptRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (transactionId == Guid.Empty)
            throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.File == null)
            throw new ArgumentException("ファイルが指定されていません", nameof(request.File));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        _logger.LogInformation(
            "レシート画像添付処理を開始します。TransactionId: {TransactionId}, UserId: {UserId}, FileName: {FileName}",
            transactionId, userId, request.File.FileName);

        // GCSアップロード用のパス
        string? uploadedPath = null;

        try
        {
            return await _transactionHelper.ExecuteInTransactionAsync(async () =>
            {
                // 1. 取引の取得
                var transaction = await _transactionReadRepository.GetByIdAsync(transactionId, cancellationToken);

                if (transaction == null)
                {
                    _logger.LogWarning("取引が見つかりません。TransactionId: {TransactionId}", transactionId);
                    throw new KeyNotFoundException($"取引が見つかりません。TransactionId: {transactionId}");
                }

                // 2. 所有権の確認
                if (transaction.UserId != userId)
                {
                    _logger.LogWarning(
                        "取引へのアクセス権限がありません。TransactionId: {TransactionId}, RequestUserId: {RequestUserId}, OwnerUserId: {OwnerUserId}",
                        transactionId, userId, transaction.UserId);
                    throw new UnauthorizedAccessException("この取引へのアクセス権限がありません");
                }

                // 3. レシート添付可否のチェック（ポリシー適用）
                var policyResult = TransactionReceiptPolicy.CanAttachReceipt(transaction);
                if (!policyResult.IsValid)
                {
                    var errorMessage = string.Join(", ", policyResult.Errors.Select(e => e.Message));
                    _logger.LogWarning(
                        "レシート添付ポリシー違反。TransactionId: {TransactionId}, Errors: {Errors}",
                        transactionId, errorMessage);
                    throw new InvalidOperationException(errorMessage);
                }

                // 4. ファイル検証
                FileValidationHelper.ValidateReceiptImageFile(request.File);

                // 5. GCSにファイルをアップロード
                var now = DateTime.UtcNow;
                var fileExtension = Path.GetExtension(request.File.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var destinationPath = $"receipts/{userId}/{now:yyyyMM}/{fileName}";

                _logger.LogDebug(
                    "GCSへのアップロードを開始します。Path: {Path}, Size: {Size} bytes",
                    destinationPath, request.File.Length);

                using var stream = request.File.OpenReadStream();
                var uploadResult = await _storageService.UploadFileAsync(
                    stream,
                    destinationPath,
                    request.File.ContentType,
                    cancellationToken: cancellationToken);

                uploadedPath = uploadResult.ObjectPath;

                _logger.LogInformation(
                    "GCSへのアップロードが完了しました。Path: {Path}",
                    uploadedPath);

                // 6. 取引にレシート画像を紐付け
                transaction.SourceUrl = uploadedPath;
                transaction.ReceiptAttachedAt = DateTimeOffset.UtcNow;
                transaction.UpdatedAt = DateTimeOffset.UtcNow;

                await _transactionWriteRepository.UpdateAsync(transaction, cancellationToken);

                _logger.LogInformation(
                    "レシート画像を取引に添付しました。TransactionId: {TransactionId}, Path: {Path}",
                    transactionId, uploadedPath);

                // 7. 結果を返す
                return new TransactionResult
                {
                    TransactionId = transaction.Id,
                    TransactionDate = transaction.TransactionDate ?? DateTimeOffset.UtcNow,
                    AmountTotal = transaction.AmountTotal ?? 0,
                    SourceUrl = transaction.SourceUrl,
                    ReceiptAttachedAt = transaction.ReceiptAttachedAt,
                    Currency = transaction.Currency,
                    Payee = transaction.Payee,
                    TaxInclusionType = transaction.TaxInclusionType,
                    Notes = transaction.Notes,
                    ProcessedAt = DateTimeOffset.UtcNow,
                    ValidationWarnings = new List<string>()
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "レシート画像添付中にエラーが発生しました。TransactionId: {TransactionId}",
                transactionId);

            // トランザクション失敗時、アップロード済みファイルを削除
            if (!string.IsNullOrEmpty(uploadedPath))
            {
                try
                {
                    await _storageService.DeleteFileAsync(uploadedPath, cancellationToken);
                    _logger.LogInformation(
                        "エラーのため、アップロード済みファイルを削除しました。Path: {Path}",
                        uploadedPath);
                }
                catch (Exception deleteEx)
                {
                    _logger.LogWarning(deleteEx,
                        "ファイルのロールバック削除に失敗しました。Path: {Path}",
                        uploadedPath);
                }
            }

            throw;
        }
    }
}
