using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto;
using ServerlessKakeibo.Api.Application.ReceiptParsing.UseCase;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ReceiptParseController : ControllerBase
{
    /// <summary>
    /// 領収書をLLMで解析して結果を返す
    /// </summary>
    /// <param name="request"></param>
    /// <param name="useCase"></param>
    /// <returns></returns>
    [HttpPost("Receipt-parse")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "領収書の画像をLLMで解析し、結果を返す",
        Description = "領収書等の画像をアップロードし、LLMで解析して内容を返す。\n\nカスタムプロンプトが設定されていないときはデフォルトのものを使用する。")]
    public async Task<ActionResult<ApiResponse<ReceiptParseResult>>> ParseReceiptAsync(
        [FromForm] ReceiptParseRequest request,
        [FromServices] IReceiptParsingUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        // ModelState の検証結果を ApiResponse 形式で返す
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(kvp => kvp.Value != null && kvp.Value.Errors.Count > 0)
                .SelectMany(kvp => kvp.Value!.Errors.Select(err => new { Key = kvp.Key, Error = err.ErrorMessage }))
                .Select(e => string.IsNullOrWhiteSpace(e.Key) ? e.Error : $"{e.Key}: {e.Error}")
                .ToList();

            var message = string.Join("; ", errors);
            return BadRequest(ApiResponse<ReceiptParseResult>.Fail(ApiStatus.InvalidRequest, message));
        }

        // ファイル未指定は400
        if (request.File is null)
        {
            return BadRequest(
                ApiResponse<ReceiptParseResult>.Fail(ApiStatus.InvalidRequest)
            );
        }

        // 画像形式以外は400
        if (request.File.ContentType is not ("image/jpeg" or "image/png"))
        {
            return BadRequest(
                ApiResponse<ReceiptParseResult>.Fail(ApiStatus.InvalidRequest)
            );
        }

        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExcuteParseAsync(request, userId);
            return Ok(ApiResponse<ReceiptParseResult>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            // ユーザーが存在しない等の認可エラーは 403 Forbidden を返す
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<ReceiptParseResult>.Fail(ApiStatus.Forbidden)
                );
            }

            return StatusCode(
                StatusCodes.Status403Forbidden,
                ApiResponse<ReceiptParseResult>.Fail(ApiStatus.Forbidden, ex.Message)
            );
        }
        catch (Exception ex)
        {
            // 開発環境以外では詳細を返さない
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<ReceiptParseResult>.Fail(ApiStatus.InternalError)
                );
            }

            // 開発環境では詳細を返す
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<ReceiptParseResult>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }
}
