using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.TransactionExport.Dto;
using ServerlessKakeibo.Api.Application.TransactionExport.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class TransactionExportController : ControllerBase
{
    /// <summary>
    /// 取引一覧をCSV+画像でエクスポート
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "取引一覧をエクスポート",
        Description = "取引データをCSV形式でエクスポートし、オプションで添付画像も含めてZipファイルで返す。\n\n" +
                      "【エクスポート内容】\n" +
                      "- transactions.csv: 取引サマリー（全フィールド）\n" +
                      "- receipts/: 添付画像（IncludeReceiptImages=trueの場合）\n" +
                      "- warnings.txt: 警告メッセージ（エラーがあった場合のみ）\n\n" +
                      "【レスポンス形式】\n" +
                      "- Data.ZipDataBase64: Base64エンコードされたZipバイナリ\n" +
                      "- Data.FileName: ファイル名\n" +
                      "- Message: 警告メッセージ（画像取得失敗時）\n\n" +
                      "【注意】\n" +
                      "- ページングは無視され、条件に一致する全件がエクスポートされます\n" +
                      "- 0件の場合は404エラーを返します\n" +
                      "- 一部の画像取得に失敗してもエクスポートは続行されます")]
    [ProducesResponseType(typeof(ApiResponse<TransactionExportResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ExportTransactionsAsync(
        [FromBody] ExportTransactionsRequest request,
        [FromServices] ITransactionExportUseCase useCase,
        [FromServices] IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.GetUserId();

            var result = await useCase.ExecuteAsync(request, userId, cancellationToken);

            // 警告メッセージを ApiResponse.Message に設定
            var warningMessage = result.Warnings.Any()
                ? string.Join("; ", result.Warnings)
                : null;

            return Ok(new ApiResponse<object>
            {
                Status = ApiStatus.Success,
                Message = warningMessage,
                Data = new
                {
                    result.FileName,
                    result.ZipDataBase64,
                    result.TotalCount,
                    result.ImagesIncludedCount,
                    result.ImagesFailedCount
                }
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponse<object>.Fail(
                    ApiStatus.Unauthorized,
                    ex.Message
                )
            );
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                ApiResponse<object>.Fail(
                    ApiStatus.NotFound,
                    ex.Message
                )
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ApiResponse<object>.Fail(
                    ApiStatus.InvalidRequest,
                    ex.Message
                )
            );
        }
        catch (Exception ex)
        {
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }
}
