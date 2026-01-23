using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using ServerlessKakeibo.Api.Common.Settings;
using ServerlessKakeibo.Api.Service.Interface;
using System.Net;
using ServerlessKakeibo.Api.Common.Exceptions;

namespace ServerlessKakeibo.Api.Service;

/// <summary>
/// Google Cloud Storage サービス
/// </summary>
public class GcpStorageService : IGcpStorageService
{
    private readonly GcpStorageSettings _settings;
    private readonly IGcpAuthService _authService;
    private readonly ILogger<GcpStorageService> _logger;
    private const string StorageScope = "https://www.googleapis.com/auth/devstorage.read_write";

    public GcpStorageService(
        IOptions<GcpStorageSettings> settings,
        IGcpAuthService authService,
        ILogger<GcpStorageService> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// ファイルをアップロードする
    /// </summary>
    public async Task<string> UploadFileAsync(Stream fileStream, string destinationPath, string contentType)
    {
        if (fileStream == null) throw new ArgumentNullException(nameof(fileStream));
        if (string.IsNullOrWhiteSpace(destinationPath)) throw new ArgumentException("保存先パスが必要です", nameof(destinationPath));

        try
        {
            _logger.LogDebug("GCSへのアップロードを開始します: {Bucket}/{Path}", _settings.BucketName, destinationPath);

            // GcpAuthServiceを使用してCredentialを取得
            var credential = await _authService.GetCredentialAsync(StorageScope);
            
            // クライアントの作成
            var storageClient = await StorageClient.CreateAsync(credential);

            // アップロード実行
            var result = await storageClient.UploadObjectAsync(
                _settings.BucketName,
                destinationPath,
                contentType,
                fileStream
            );

            _logger.LogInformation("GCSへのアップロードが完了しました: {Path}", result.Name);

            return result.Name;
        }
        catch (CustomException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GCSへのアップロード中にエラーが発生しました。パス: {Path}", destinationPath);
            throw new CustomException(
                new ExceptionType(HttpStatusCode.InternalServerError, "ストレージへの保存に失敗しました"), ex);
        }
    }

    /// <summary>
    /// ファイルを削除する
    /// </summary>
    public async Task DeleteFileAsync(string objectPath)
    {
        if (string.IsNullOrWhiteSpace(objectPath)) return;

        try
        {
            var credential = await _authService.GetCredentialAsync(StorageScope);
            var storageClient = await StorageClient.CreateAsync(credential);

            await storageClient.DeleteObjectAsync(_settings.BucketName, objectPath);
            _logger.LogInformation("GCSのファイルを削除しました: {Path}", objectPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GCSのファイル削除に失敗しました（無視して続行）: {Path}", objectPath);
        }
    }
}
