using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.TransactionSummary.Dto;
using ServerlessKakeibo.Api.Application.TransactionSummary.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// 取引サマリーコントローラー
/// </summary>
[ApiController]
[Route("[controller]")]
[Authorize]
public class TransactionSummaryController : ControllerBase
{
    /// <summary>
    /// 月次サマリーを取得
    /// </summary>
    /// <param name="request">月次サマリー取得リクエスト</param>
    /// <param name="useCase">月次サマリーユースケース</param>
    /// <param name="environment">ホスト環境</param>
    /// <returns>月次サマリー結果</returns>
    [HttpGet("monthly")]
    [SwaggerOperation(
        Summary = "月次サマリーを取得",
        Description = "指定された年月の収支サマリーを取得する。\n\n" +
                      "収入合計、支出合計、差引（収入-支出）、支出トップ3カテゴリを返す。\n\n" +
                      "差引が正の値なら黒字、負の値なら赤字、0なら収支ゼロ。")]
    [ProducesResponseType(typeof(ApiResponse<MonthlySummaryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MonthlySummaryResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MonthlySummaryResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<MonthlySummaryResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<MonthlySummaryResult>>> GetMonthlySummaryAsync(
        [FromQuery] GetMonthlySummaryRequest request,
        [FromServices] IMonthlySummaryUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            var userId = User.GetUserId();

            var result = await useCase.GetMonthlySummaryAsync(
                request.Year,
                request.Month,
                userId);

            return Ok(ApiResponse<MonthlySummaryResult>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponse<MonthlySummaryResult>.Fail(
                    ApiStatus.Unauthorized,
                    ex.Message
                )
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ApiResponse<MonthlySummaryResult>.Fail(
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
                    ApiResponse<MonthlySummaryResult>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<MonthlySummaryResult>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }
}
