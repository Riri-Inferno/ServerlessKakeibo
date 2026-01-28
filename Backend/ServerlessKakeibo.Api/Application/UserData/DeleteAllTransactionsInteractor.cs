using ServerlessKakeibo.Api.Application.UserData.Dto;
using ServerlessKakeibo.Api.Application.UserData.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Service.Interface;

namespace ServerlessKakeibo.Api.Application.UserData
{
    /// <summary>
    /// 全取引データ削除インタラクター
    /// </summary>
    public class DeleteAllTransactionsInteractor : IDeleteAllTransactionsUseCase
    {
        private readonly ITransactionHelper _transactionHelper;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IGcpStorageService _storageService;
        private readonly ILogger<DeleteAllTransactionsInteractor> _logger;

        public DeleteAllTransactionsInteractor(
            ITransactionHelper transactionHelper,
            ITransactionRepository transactionRepository,
            IGcpStorageService storageService,
            ILogger<DeleteAllTransactionsInteractor> logger)
        {
            _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
            _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// ユーザーに紐づく全取引データを論理削除
        /// </summary>
        public async Task<DeleteAllTransactionsResult> ExecuteAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("ユーザーIDが無効です", nameof(userId));

            _logger.LogInformation(
                "全取引データの削除を開始します。UserId: {UserId}",
                userId);

            var result = new DeleteAllTransactionsResult
            {
                CompletedAt = DateTimeOffset.UtcNow
            };

            try
            {
                // 1. GCS削除用に画像URLを先に取得
                List<string> imageUrls = new();

                await _transactionHelper.ExecuteInTransactionAsync(async () =>
                {
                    // 1-1. 削除前に画像URLリストを取得
                    imageUrls = await _transactionRepository.GetAllReceiptImageUrlsAsync(
                        userId,
                        cancellationToken);

                    _logger.LogInformation(
                        "削除対象の画像数: {Count}件",
                        imageUrls.Count);

                    // 1-2. 全取引データを一括論理削除
                    var (transactions, items, taxes, shops) = await _transactionRepository
                        .SoftDeleteAllUserTransactionsAsync(userId, cancellationToken);

                    result.DeletedTransactionCount = transactions;
                    result.DeletedTransactionItemCount = items;
                    result.DeletedTaxDetailCount = taxes;
                    result.DeletedShopDetailCount = shops;

                    _logger.LogInformation(
                        "DB削除完了。取引: {Transactions}件, 明細: {Items}件, 税: {Taxes}件, 店舗: {Shops}件",
                        transactions, items, taxes, shops);
                });

                // 2. トランザクションコミット後、GCSから画像削除
                if (imageUrls.Any())
                {
                    await DeleteReceiptImagesAsync(imageUrls, result, cancellationToken);
                }

                _logger.LogInformation(
                    "全取引データの削除が完了しました。UserId: {UserId}, " +
                    "削除取引数: {Transactions}, 削除画像数: {Images}, 失敗画像数: {Failed}",
                    userId,
                    result.DeletedTransactionCount,
                    result.DeletedImageCount,
                    result.FailedImageCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "全取引データの削除中にエラーが発生しました。UserId: {UserId}",
                    userId);
                throw;
            }

            return result;
        }

        /// <summary>
        /// GCSからレシート画像を削除
        /// </summary>
        private async Task DeleteReceiptImagesAsync(
            List<string> imageUrls,
            DeleteAllTransactionsResult result,
            CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation(
                    "GCSから画像を削除します。対象数: {Count}件",
                    imageUrls.Count);

                // URLまたはパスからオブジェクトパスを抽出
                var objectPaths = imageUrls
                    .Select(urlOrPath => NormalizeToObjectPath(urlOrPath))
                    .Where(path => !string.IsNullOrEmpty(path))
                    .Cast<string>()
                    .ToList();

                if (!objectPaths.Any())
                {
                    _logger.LogWarning("有効な画像パスが抽出できませんでした");
                    return;
                }

                _logger.LogDebug(
                    "抽出成功: {SuccessCount}件 / {TotalCount}件",
                    objectPaths.Count,
                    imageUrls.Count);

                // 一括削除
                var deleteResult = await _storageService.DeleteFilesAsync(
                    objectPaths,
                    cancellationToken);

                result.DeletedImageCount = deleteResult.SuccessCount;
                result.FailedImageCount = deleteResult.FailureCount;
                result.FailedImagePaths = deleteResult.Errors
                    .Select(e => e.ObjectPath)
                    .ToList();

                // 失敗した画像のログ出力
                foreach (var error in deleteResult.Errors)
                {
                    _logger.LogWarning(
                        "画像削除失敗: Path={Path}, Error={Error}, StatusCode={StatusCode}",
                        error.ObjectPath,
                        error.ErrorMessage,
                        error.StatusCode);
                }

                _logger.LogInformation(
                    "GCS画像削除完了。成功: {Success}件, 失敗: {Failed}件",
                    result.DeletedImageCount,
                    result.FailedImageCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "GCS画像削除中にエラーが発生しました");

                // GCS削除失敗はログのみで例外を再スローしない
                result.FailedImageCount = imageUrls.Count;
                result.FailedImagePaths = imageUrls;
            }
        }

        /// <summary>
        /// URL形式またはパス形式をオブジェクトパスに正規化
        /// </summary>
        /// <param name="urlOrPath">GCS URLまたはオブジェクトパス</param>
        /// <returns>オブジェクトパス</returns>
        private string? NormalizeToObjectPath(string urlOrPath)
        {
            if (string.IsNullOrWhiteSpace(urlOrPath))
                return null;

            try
            {
                // 既にオブジェクトパス形式（httpで始まらない）の場合はそのまま返す
                if (!urlOrPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("オブジェクトパス形式を検出: {Path}", urlOrPath);
                    return urlOrPath;
                }

                // URL形式の場合はパースしてパスを抽出
                _logger.LogDebug("URL形式からパースします: {Url}", urlOrPath);

                var uri = new Uri(urlOrPath);

                // パスから最初のスラッシュとバケット名を除去
                var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                if (segments.Length < 2)
                {
                    _logger.LogWarning(
                        "不正なGCS URLフォーマット: {Url}",
                        urlOrPath);
                    return null;
                }

                // バケット名（最初のセグメント）をスキップして結合
                var objectPath = string.Join("/", segments.Skip(1));

                _logger.LogDebug("URL形式から抽出完了: {Url} -> {Path}", urlOrPath, objectPath);

                return objectPath;
            }
            catch (UriFormatException ex)
            {
                _logger.LogWarning(
                    ex,
                    "URL/パスのパース失敗: {UrlOrPath}",
                    urlOrPath);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "予期しないエラー: {UrlOrPath}",
                    urlOrPath);
                return null;
            }
        }
    }
}
