using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.TransactionQuery.Dto;
using ServerlessKakeibo.Api.Application.TransactionQuery.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class TransactionController : ControllerBase
{
    /// <summary>
    /// 取引詳細を取得
    /// </summary>
    /// <param name="id">取引ID</param>
    /// <param name="useCase">取引クエリユースケース</param>
    /// <param name="environment">ホスト環境</param>
    /// <returns>取引詳細</returns>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "取引詳細を取得",
        Description = "指定されたIDの取引詳細を取得する。\n\n取引項目、税情報、店舗詳細を含む完全なデータを返す。")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDetailResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransactionDetailResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<TransactionDetailResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TransactionDetailResult>>> GetTransactionByIdAsync(
        [FromRoute] Guid id,
        [FromServices] ITransactionQueryUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            // TODO: 認証実装後はトークンからユーザーIDを取得
            var userId = Guid.Parse("a1111111-1111-1111-1111-111111111111");

            var result = await useCase.GetByIdAsync(id, userId);

            if (result == null)
            {
                return NotFound(
                    ApiResponse<TransactionDetailResult>.Fail(
                        ApiStatus.NotFound,
                        "指定された取引が見つかりません"
                    )
                );
            }

            return Ok(ApiResponse<TransactionDetailResult>.Success(result));
        }
        catch (ArgumentException)
        {
            return BadRequest(
                ApiResponse<TransactionDetailResult>.Fail(ApiStatus.InvalidRequest)
            );
        }
        catch (Exception ex)
        {
            // 開発環境以外では詳細を返さない
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<TransactionDetailResult>.Fail(ApiStatus.InternalError)
                );
            }

            // 開発環境では詳細を返す
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<TransactionDetailResult>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }
}
