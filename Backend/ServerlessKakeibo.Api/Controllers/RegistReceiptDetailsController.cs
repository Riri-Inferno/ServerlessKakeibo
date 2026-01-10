using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.RegistReceiptDetails.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class RegistReceiptDetailsController : ControllerBase
{
    /// <summary>
    /// 領収書解析結果を保存
    /// </summary>
    [HttpPost("parse-and-save")]
    [SwaggerOperation(
        Summary = "領収書解析結果を保存",
        Description = "解析済みの領収書データをトランザクションとして保存する。")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> SaveTransactionAsync(
        [FromBody] SaveReceiptParseResultRequest request,
        [FromServices] IRegistReceiptDetailsUseCase useCase,
        [FromServices] IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: 認証実装後はトークンからユーザーIDを取得
            var userId = Guid.Parse("a1111111-1111-1111-1111-111111111111");

            var result = await useCase.ExecuteSaveAsync(request, userId, cancellationToken);

            return Ok(ApiResponse<object>.Success(result));
        }
        catch (ArgumentException)
        {
            return BadRequest(
                ApiResponse<object>.Fail(ApiStatus.InvalidRequest)
            );
        }
        catch (InvalidOperationException ex)
        {
            // 開発環境以外では詳細を返さない
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(ApiStatus.InternalError)
                );
            }

            // 開発環境では詳細を返す
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail(ApiStatus.InternalError, ex.ToString())
            );
        }
        catch (Exception ex)
        {
            // 開発環境以外では詳細を返さない
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(ApiStatus.InternalError)
                );
            }

            // 開発環境では詳細を返す
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail(ApiStatus.InternalError, ex.ToString())
            );
        }
    }
}
