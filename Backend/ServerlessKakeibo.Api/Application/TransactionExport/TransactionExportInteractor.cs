using System.IO.Compression;
using ServerlessKakeibo.Api.Application.TransactionExport.Components;
using ServerlessKakeibo.Api.Application.TransactionExport.Dto;
using ServerlessKakeibo.Api.Application.TransactionExport.Mappers;
using ServerlessKakeibo.Api.Application.TransactionExport.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Service.Interface;

namespace ServerlessKakeibo.Api.Application.TransactionExport;

/// <summary>
/// 取引エクスポートインタラクター
/// </summary>
public class TransactionExportInteractor : ITransactionExportUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IGcpStorageService _storageService;
    private readonly ILogger<TransactionExportInteractor> _logger;

    public TransactionExportInteractor(
        ITransactionRepository transactionRepository,
        IGcpStorageService storageService,
        ILogger<TransactionExportInteractor> logger)
    {
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 取引データをエクスポート
    /// </summary>
    public async Task<TransactionExportResult> ExecuteAsync(
        ExportTransactionsRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        _logger.LogInformation(
            "取引エクスポート開始: UserId={UserId}, IncludeImages={IncludeImages}",
            userId, request.IncludeReceiptImages);

        // 1. 取引データ取得
        var transactions = await _transactionRepository.GetAllForExportAsync(
            userId,
            request.StartDate,
            request.EndDate,
            request.Category,
            request.Payee,
            request.MinAmount,
            request.MaxAmount,
            request.Type,
            cancellationToken);

        if (!transactions.Any())
        {
            _logger.LogWarning("エクスポート対象の取引が0件です: UserId={UserId}", userId);
            throw new KeyNotFoundException("指定された条件に一致する取引が見つかりません");
        }

        // 2. CSV生成
        var exportDtos = transactions.Select(TransactionExportMapper.ToExportDto).ToList();
        var csvData = CsvGeneratorComponent.GenerateCsv(exportDtos);

        // 3. Zip生成
        var result = await CreateZipArchiveAsync(
            csvData,
            transactions,
            request.IncludeReceiptImages,
            cancellationToken);

        _logger.LogInformation(
            "取引エクスポート完了: UserId={UserId}, Count={Count}, ImagesIncluded={ImagesIncluded}, ImagesFailed={ImagesFailed}",
            userId, result.TotalCount, result.ImagesIncludedCount, result.ImagesFailedCount);

        return result;
    }

    /// <summary>
    /// Zipアーカイブを作成
    /// </summary>
    private async Task<TransactionExportResult> CreateZipArchiveAsync(
        byte[] csvData,
        List<TransactionEntity> transactions,
        bool includeImages,
        CancellationToken cancellationToken)
    {
        var result = new TransactionExportResult
        {
            TotalCount = transactions.Count,
            FileName = $"transactions_{DateTime.Now:yyyyMMddHHmmss}.zip"
        };

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // 1. CSVを追加
            var csvEntry = archive.CreateEntry("transactions.csv", CompressionLevel.Optimal);
            using (var entryStream = csvEntry.Open())
            {
                await entryStream.WriteAsync(csvData, 0, csvData.Length, cancellationToken);
            }

            // 2. 画像を追加（オプション）
            if (includeImages)
            {
                await AddReceiptImagesToArchiveAsync(
                    archive,
                    transactions,
                    result,
                    cancellationToken);
            }

            // 3. 警告がある場合はwarnings.txtを追加
            if (result.Warnings.Any())
            {
                var warningText = "【警告】\n\n" + string.Join("\n", result.Warnings);
                var warningBytes = System.Text.Encoding.UTF8.GetBytes(warningText);

                var warningEntry = archive.CreateEntry("warnings.txt", CompressionLevel.Optimal);
                using var warningStream = warningEntry.Open();
                await warningStream.WriteAsync(warningBytes, 0, warningBytes.Length, cancellationToken);
            }
        }

        memoryStream.Position = 0;
        var zipBytes = memoryStream.ToArray();

        // Base64エンコード
        result.ZipDataBase64 = Convert.ToBase64String(zipBytes);

        return result;
    }

    /// <summary>
    /// レシート画像をZipに追加
    /// </summary>
    private async Task AddReceiptImagesToArchiveAsync(
        ZipArchive archive,
        List<TransactionEntity> transactions,
        TransactionExportResult result,
        CancellationToken cancellationToken)
    {
        var transactionsWithImages = transactions
            .Where(t => !string.IsNullOrWhiteSpace(t.SourceUrl))
            .ToList();

        _logger.LogInformation("画像付き取引: {Count}件", transactionsWithImages.Count);

        foreach (var transaction in transactionsWithImages)
        {
            try
            {
                // GCSから画像をダウンロード
                var imageData = await DownloadImageFromGcsAsync(
                    transaction.SourceUrl!,
                    cancellationToken);

                if (imageData == null || imageData.Length == 0)
                {
                    result.ImagesFailedCount++;
                    _logger.LogWarning(
                        "画像データが空です: TransactionId={TransactionId}, Path={Path}",
                        transaction.Id, transaction.SourceUrl);
                    continue;
                }

                // ファイル名: {TransactionId}.{拡張子}
                var extension = Path.GetExtension(transaction.SourceUrl);
                var fileName = $"receipts/{transaction.Id}{extension}";

                // Zipに追加
                var imageEntry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                using var entryStream = imageEntry.Open();
                await entryStream.WriteAsync(imageData, 0, imageData.Length, cancellationToken);

                result.ImagesIncludedCount++;
            }
            catch (Exception ex)
            {
                result.ImagesFailedCount++;
                _logger.LogError(ex,
                    "画像取得失敗: TransactionId={TransactionId}, Path={Path}",
                    transaction.Id, transaction.SourceUrl);
            }
        }

        // 警告メッセージ生成
        if (result.ImagesFailedCount > 0)
        {
            result.Warnings.Add($"一部の画像を取得できませんでした（{result.ImagesFailedCount}件）");
        }
    }

    /// <summary>
    /// GCSから画像をダウンロード
    /// </summary>
    private async Task<byte[]?> DownloadImageFromGcsAsync(
        string objectPath,
        CancellationToken cancellationToken)
    {
        var signedUrl = await _storageService.GenerateSignedUrlAsync(
            objectPath,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(signedUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "GCS画像取得失敗: Path={Path}, StatusCode={StatusCode}",
                objectPath, response.StatusCode);
            return null;
        }

        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }
}
