using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Service.Interface;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// 【検証用】ストレージ動作確認コントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StorageController : ControllerBase
{
    private readonly IGcpStorageService _storageService;
    private readonly ILogger<StorageController> _logger;

    public StorageController(
        IGcpStorageService storageService,
        ILogger<StorageController> logger)
    {
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// ファイルをGCSにアップロードテスト
    /// </summary>
    /// <param name="file">画像等のファイル</param>
    [HttpPost("upload")]
    [SwaggerOperation(Summary = "GCSアップロードテスト", Description = "選択したファイルをGCSの 'test-uploads/' フォルダに保存します。")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> UploadTestAsync(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<string>.Fail(ApiStatus.InvalidRequest, "ファイルが選択されていません。"));
            }

            _logger.LogInformation("テストアップロード開始: {FileName}", file.FileName);

            // test-uploads フォルダに GUID名で保存
            var extension = Path.GetExtension(file.FileName);
            var destinationPath = $"test-uploads/{Guid.NewGuid()}{extension}";

            using var stream = file.OpenReadStream();
            var resultPath = await _storageService.UploadFileAsync(
                stream,
                destinationPath,
                file.ContentType);

            _logger.LogInformation("テストアップロード成功: {Path}", resultPath);

            return Ok(ApiResponse<string>.Success(resultPath));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "アップロードテスト中にエラーが発生しました");
            return StatusCode(500, ApiResponse<string>.Fail(ApiStatus.InternalError, ex.Message));
        }
    }

    /// <summary>
    /// ファイルをGCSから削除テスト
    /// </summary>
    /// <param name="objectPath">削除するオブジェクトのパス</param>
    [HttpDelete("delete")]
    [SwaggerOperation(Summary = "GCS削除テスト", Description = "指定したパスのオブジェクトをGCSから削除します。")]
    public async Task<ActionResult<ApiResponse<string>>> DeleteTestAsync([FromQuery] string objectPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(objectPath))
            {
                return BadRequest(ApiResponse<string>.Fail(ApiStatus.InvalidRequest, "パスが空です。"));
            }

            await _storageService.DeleteFileAsync(objectPath);
            return Ok(ApiResponse<string>.Success($"Deleted: {objectPath}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "削除テスト中にエラーが発生しました");
            return StatusCode(500, ApiResponse<string>.Fail(ApiStatus.InternalError, ex.Message));
        }
    }
}
