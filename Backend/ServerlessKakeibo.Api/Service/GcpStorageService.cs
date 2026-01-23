using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using ServerlessKakeibo.Api.Common.Settings;
using ServerlessKakeibo.Api.Service.Interface;
using ServerlessKakeibo.Api.Service.Models;
using System.Net;
using ServerlessKakeibo.Api.Common.Exceptions;

namespace ServerlessKakeibo.Api.Service;

/// <summary>
/// Google Cloud Storage サービス実装
/// </summary>
public class GcpStorageService : IGcpStorageService
{
    private readonly GcpStorageSettings _settings;
    private readonly IGcpAuthService _authService;
    private readonly ILogger<GcpStorageService> _logger;
    private const string StorageScope = "https://www.googleapis.com/auth/devstorage.read_write";

    private StorageClient? _storageClient;

    public GcpStorageService(
        IOptions<GcpStorageSettings> settings,
        IGcpAuthService authService,
        ILogger<GcpStorageService> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        ValidateSettings();
    }

    /// <summary>
    /// 設定値の検証
    /// </summary>
    private void ValidateSettings()
    {
        if (string.IsNullOrWhiteSpace(_settings.BucketName))
            throw new InvalidOperationException("GcpStorageSettings.BucketName が設定されていません");

        if (string.IsNullOrWhiteSpace(_settings.ProjectId))
            throw new InvalidOperationException("GcpStorageSettings.ProjectId が設定されていません");

        _logger.LogDebug(
            "GcpStorageService初期化完了。Bucket: {Bucket}, Project: {Project}",
            _settings.BucketName, _settings.ProjectId);
    }

    /// <summary>
    /// StorageClientを取得（遅延初期化）
    /// </summary>
    private async Task<StorageClient> GetStorageClientAsync()
    {
        if (_storageClient == null)
        {
            _logger.LogDebug("StorageClientを初期化します");

            var credential = await _authService.GetCredentialAsync(StorageScope);
            _storageClient = await StorageClient.CreateAsync(credential);

            _logger.LogDebug("StorageClientの初期化が完了しました");
        }

        return _storageClient;
    }

    /// <summary>
    /// ファイルをアップロードする
    /// </summary>
    public async Task<GcsUploadResult> UploadFileAsync(
        Stream fileStream,
        string destinationPath,
        string contentType,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (fileStream == null)
            throw new ArgumentNullException(nameof(fileStream));

        if (string.IsNullOrWhiteSpace(destinationPath))
            throw new ArgumentException("保存先パスが必要です", nameof(destinationPath));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("コンテンツタイプが必要です", nameof(contentType));

        try
        {
            _logger.LogDebug(
                "GCSへのアップロードを開始します: {Bucket}/{Path}, ContentType: {ContentType}",
                _settings.BucketName, destinationPath, contentType);

            var storageClient = await GetStorageClientAsync();

            // アップロードするオブジェクトの定義
            var uploadObject = new Google.Apis.Storage.v1.Data.Object
            {
                Bucket = _settings.BucketName,
                Name = destinationPath,
                ContentType = contentType,
                Metadata = metadata ?? new Dictionary<string, string>()
            };

            // 自動メタデータの追加
            uploadObject.Metadata["uploaded_at"] = DateTimeOffset.UtcNow.ToString("o");
            uploadObject.Metadata["original_path"] = destinationPath;

            // アップロード実行
            var uploadedObject = await storageClient.UploadObjectAsync(
                uploadObject,
                fileStream,
                cancellationToken: cancellationToken);

            // 結果の構築
            var result = new GcsUploadResult
            {
                ObjectPath = uploadedObject.Name,
                ContentType = uploadedObject.ContentType,
                Size = (long)(uploadedObject.Size ?? 0),
                UploadedAt = DateTimeOffset.UtcNow,
                Metadata = uploadedObject.Metadata ?? new Dictionary<string, string>()
            };

            // 公開URLの生成（バケットが公開設定の場合）
            if (_settings.IsPublicBucket)
            {
                result.PublicUrl = $"https://storage.googleapis.com/{_settings.BucketName}/{uploadedObject.Name}";
            }

            // 署名付きURLの生成（設定で有効な場合）
            if (_settings.GenerateSignedUrl)
            {
                try
                {
                    result.SignedUrl = await GenerateSignedUrlAsync(
                        uploadedObject.Name,
                        TimeSpan.FromHours(_settings.SignedUrlExpirationHours),
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "署名付きURLの生成に失敗しました: {Path}", uploadedObject.Name);
                }
            }

            _logger.LogInformation(
                "GCSへのアップロードが完了しました。Path: {Path}, Size: {Size} bytes",
                result.ObjectPath, result.Size);

            return result;
        }
        catch (Google.GoogleApiException gex)
        {
            var errorMessage = gex.HttpStatusCode switch
            {
                HttpStatusCode.Forbidden => "ストレージへのアクセス権限がありません",
                HttpStatusCode.NotFound => "指定されたバケットが存在しません",
                HttpStatusCode.RequestEntityTooLarge => "ファイルサイズが上限を超えています",
                HttpStatusCode.InsufficientStorage => "ストレージの容量が不足しています",
                HttpStatusCode.PreconditionFailed => "ファイルの事前条件が満たされていません",
                _ => $"ストレージへの保存に失敗しました: {gex.Message}"
            };

            _logger.LogError(gex,
                "GCS API エラーが発生しました。StatusCode: {StatusCode}, Message: {Message}, Path: {Path}",
                gex.HttpStatusCode, gex.Message, destinationPath);

            throw new CustomException(
                new ExceptionType(
                    (HttpStatusCode)gex.HttpStatusCode,
                    errorMessage),
                gex);
        }
        catch (CustomException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ファイルアップロード中に予期しないエラーが発生しました。Path: {Path}",
                destinationPath);

            throw new CustomException(
                new ExceptionType(
                    HttpStatusCode.InternalServerError,
                    "ファイルのアップロード中に予期しないエラーが発生しました"),
                ex);
        }
    }

    /// <summary>
    /// ファイルを削除する
    /// </summary>
    public async Task<bool> DeleteFileAsync(
        string objectPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectPath))
        {
            _logger.LogWarning("削除対象のパスが空です");
            return false;
        }

        try
        {
            _logger.LogDebug("GCSファイルの削除を開始します: {Bucket}/{Path}",
                _settings.BucketName, objectPath);

            var storageClient = await GetStorageClientAsync();

            await storageClient.DeleteObjectAsync(
                _settings.BucketName,
                objectPath,
                cancellationToken: cancellationToken);

            _logger.LogInformation("GCSのファイルを削除しました: {Path}", objectPath);

            return true;
        }
        catch (Google.GoogleApiException gex) when (gex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning(
                "削除対象のファイルが見つかりません（既に削除済みの可能性）: {Path}",
                objectPath);

            return false;
        }
        catch (Google.GoogleApiException gex)
        {
            var errorMessage = gex.HttpStatusCode switch
            {
                HttpStatusCode.Forbidden => "ファイルの削除権限がありません",
                HttpStatusCode.PreconditionFailed => "ファイルの削除条件が満たされていません",
                _ => $"ファイルの削除に失敗しました: {gex.Message}"
            };

            _logger.LogError(gex,
                "GCS API エラーが発生しました。StatusCode: {StatusCode}, Path: {Path}",
                gex.HttpStatusCode, objectPath);

            throw new CustomException(
                new ExceptionType(
                    (HttpStatusCode)gex.HttpStatusCode,
                    errorMessage),
                gex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "ファイル削除中に予期しないエラーが発生しました。Path: {Path}",
                objectPath);

            throw new CustomException(
                new ExceptionType(
                    HttpStatusCode.InternalServerError,
                    "ファイルの削除中に予期しないエラーが発生しました"),
                ex);
        }
    }

    /// <summary>
    /// 複数ファイルを一括削除する
    /// </summary>
    public async Task<GcsBatchDeleteResult> DeleteFilesAsync(
        IEnumerable<string> objectPaths,
        CancellationToken cancellationToken = default)
    {
        if (objectPaths == null)
            throw new ArgumentNullException(nameof(objectPaths));

        var result = new GcsBatchDeleteResult();
        var pathList = objectPaths.ToList();

        if (!pathList.Any())
        {
            _logger.LogWarning("削除対象のパスが指定されていません");
            return result;
        }

        _logger.LogInformation("バッチ削除を開始します。対象ファイル数: {Count}", pathList.Count);

        var storageClient = await GetStorageClientAsync();

        // 並列削除処理
        var deleteTasks = pathList.Select(async path =>
        {
            try
            {
                await storageClient.DeleteObjectAsync(
                    _settings.BucketName,
                    path,
                    cancellationToken: cancellationToken);

                result.IncrementSuccess();

                _logger.LogDebug("ファイル削除成功: {Path}", path);

                return (path, success: true, error: (GcsDeleteError?)null);
            }
            catch (Google.GoogleApiException gex)
            {
                result.IncrementFailure();

                var deleteError = new GcsDeleteError
                {
                    ObjectPath = path,
                    ErrorMessage = gex.Message,
                    StatusCode = (int)gex.HttpStatusCode
                };

                lock (result.Errors)
                {
                    result.Errors.Add(deleteError);
                }

                _logger.LogWarning(gex, "ファイル削除失敗: {Path}, StatusCode: {StatusCode}",
                    path, gex.HttpStatusCode);

                return (path, success: false, error: deleteError);
            }
            catch (Exception ex)
            {
                result.IncrementFailure();

                var deleteError = new GcsDeleteError
                {
                    ObjectPath = path,
                    ErrorMessage = ex.Message
                };

                lock (result.Errors)
                {
                    result.Errors.Add(deleteError);
                }

                _logger.LogWarning(ex, "ファイル削除失敗（予期しないエラー）: {Path}", path);

                return (path, success: false, error: deleteError);
            }
        });

        await Task.WhenAll(deleteTasks);

        _logger.LogInformation(
            "バッチ削除完了。成功: {Success}, 失敗: {Failure}",
            result.SuccessCount, result.FailureCount);

        return result;
    }

    /// <summary>
    /// ファイルの存在を確認する
    /// </summary>
    public async Task<bool> FileExistsAsync(
        string objectPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectPath))
        {
            _logger.LogWarning("確認対象のパスが空です");
            return false;
        }

        try
        {
            var storageClient = await GetStorageClientAsync();

            await storageClient.GetObjectAsync(
                _settings.BucketName,
                objectPath,
                cancellationToken: cancellationToken);

            _logger.LogDebug("ファイルが存在します: {Path}", objectPath);

            return true;
        }
        catch (Google.GoogleApiException gex) when (gex.HttpStatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug("ファイルが存在しません: {Path}", objectPath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "ファイル存在確認中にエラーが発生しました: {Path}", objectPath);
            return false;
        }
    }

    /// <summary>
    /// 署名付き一時URLを生成する（プライベートバケット用）
    /// </summary>
    public async Task<string> GenerateSignedUrlAsync(
        string objectPath,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectPath))
            throw new ArgumentException("オブジェクトパスが必要です", nameof(objectPath));

        if (expiration <= TimeSpan.Zero)
            throw new ArgumentException("有効期限は正の値である必要があります", nameof(expiration));

        try
        {
            _logger.LogDebug(
                "署名付きURLを生成します。Path: {Path}, Expiration: {Expiration}",
                objectPath, expiration);

            var credential = await _authService.GetCredentialAsync(StorageScope);
            var urlSigner = UrlSigner.FromCredential(credential);

            var signedUrl = await urlSigner.SignAsync(
                _settings.BucketName,
                objectPath,
                expiration,
                HttpMethod.Get);

            _logger.LogDebug("署名付きURLの生成が完了しました: {Path}", objectPath);

            return signedUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "署名付きURL生成中にエラーが発生しました。Path: {Path}",
                objectPath);

            throw new CustomException(
                new ExceptionType(
                    HttpStatusCode.InternalServerError,
                    "一時URLの生成に失敗しました"),
                ex);
        }
    }
}
