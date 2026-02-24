using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.Statistics.Dto;
using ServerlessKakeibo.Api.Application.Statistics.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// 統計情報コントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatisticsController : ControllerBase
{
    /// <summary>
    /// 前月比込みの月次サマリーを取得
    /// </summary>
    /// <param name="request">年月指定</param>
    /// <param name="useCase">統計ユースケース</param>
    /// <param name="environment">ホスト環境</param>
    /// <returns>前月比込みサマリー</returns>
    [HttpGet("monthly-comparison")]
    [SwaggerOperation(
        Summary = "前月比込み月次サマリーを取得",
        Description = "指定月と前月の収支を比較し、前月比（%）を含めて返す。\n\n" +
                      "前月データがない場合、Previous は null になります。")]
    [ProducesResponseType(typeof(ApiResponse<MonthlyComparisonResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MonthlyComparisonResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MonthlyComparisonResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<MonthlyComparisonResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<MonthlyComparisonResult>>> GetMonthlyComparison(
        [FromQuery] GetMonthlySummaryRequest request,
        [FromServices] IStatisticsUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            var userId = User.GetUserId();

            var result = await useCase.GetMonthlyComparisonAsync(
                request.Year,
                request.Month,
                userId);

            return Ok(ApiResponse<MonthlyComparisonResult>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponse<MonthlyComparisonResult>.Fail(
                    ApiStatus.Unauthorized,
                    ex.Message
                )
            );
        }
        catch (ArgumentException)
        {
            return BadRequest(
                ApiResponse<MonthlyComparisonResult>.Fail(ApiStatus.InvalidRequest)
            );
        }
        catch (Exception ex)
        {
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<MonthlyComparisonResult>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<MonthlyComparisonResult>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }

    /// <summary>
    /// カテゴリ別支出内訳を取得
    /// </summary>
    /// <param name="request">年月指定</param>
    /// <param name="useCase">統計ユースケース</param>
    /// <param name="environment">ホスト環境</param>
    /// <returns>全カテゴリの支出内訳</returns>
    [HttpGet("category-breakdown")]
    [SwaggerOperation(
        Summary = "カテゴリ別支出内訳を取得",
        Description = "指定月の全カテゴリの支出を取得し、金額と割合を返す。\n\n" +
                      "円グラフ表示用に全カテゴリを返します（TopN制限なし）。")]
    [ProducesResponseType(typeof(ApiResponse<CategoryBreakdownResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CategoryBreakdownResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CategoryBreakdownResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<CategoryBreakdownResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CategoryBreakdownResult>>> GetCategoryBreakdown(
        [FromQuery] GetMonthlySummaryRequest request,
        [FromServices] IStatisticsUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            var userId = User.GetUserId();

            var result = await useCase.GetCategoryBreakdownAsync(
                request.Year,
                request.Month,
                userId);

            return Ok(ApiResponse<CategoryBreakdownResult>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponse<CategoryBreakdownResult>.Fail(
                    ApiStatus.Unauthorized,
                    ex.Message
                )
            );
        }
        catch (ArgumentException)
        {
            return BadRequest(
                ApiResponse<CategoryBreakdownResult>.Fail(ApiStatus.InvalidRequest)
            );
        }
        catch (Exception ex)
        {
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<CategoryBreakdownResult>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<CategoryBreakdownResult>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }

    /// <summary>
    /// 月次推移を取得
    /// </summary>
    /// <param name="request">取得月数</param>
    /// <param name="useCase">統計ユースケース</param>
    /// <param name="environment">ホスト環境</param>
    /// <returns>直近N ヶ月の収入・支出推移</returns>
    [HttpGet("trend")]
    [SwaggerOperation(
        Summary = "月次推移を取得",
        Description = "直近N ヶ月の収入・支出・収支を取得し、グラフ表示用データを返す。\n\n" +
                      "デフォルトは直近240ヶ月です。")]
    [ProducesResponseType(typeof(ApiResponse<MonthlyTrendResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MonthlyTrendResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MonthlyTrendResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<MonthlyTrendResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<MonthlyTrendResult>>> GetMonthlyTrend(
        [FromQuery] GetMonthlyTrendRequest request,
        [FromServices] IStatisticsUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            var userId = User.GetUserId();

            var result = await useCase.GetMonthlyTrendAsync(
                request.Months,
                userId);

            return Ok(ApiResponse<MonthlyTrendResult>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponse<MonthlyTrendResult>.Fail(
                    ApiStatus.Unauthorized,
                    ex.Message
                )
            );
        }
        catch (ArgumentException)
        {
            return BadRequest(
                ApiResponse<MonthlyTrendResult>.Fail(ApiStatus.InvalidRequest)
            );
        }
        catch (Exception ex)
        {
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<MonthlyTrendResult>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<MonthlyTrendResult>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }

    /// <summary>
    /// 月次ハイライトを取得
    /// </summary>
    /// <param name="request">年月指定</param>
    /// <param name="useCase">統計ユースケース</param>
    /// <param name="environment">ホスト環境</param>
    /// <returns>最高額取引、最多カテゴリなど</returns>
    [HttpGet("highlights")]
    [SwaggerOperation(
        Summary = "月次ハイライトを取得",
        Description = "指定月の統計ハイライトを取得。\n\n" +
                      "- 最高額の支出取引\n" +
                      "- 最も頻度の高いカテゴリ\n" +
                      "- 1日あたり平均支出\n" +
                      "- 支出があった日数")]
    [ProducesResponseType(typeof(ApiResponse<HighlightsResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<HighlightsResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<HighlightsResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<HighlightsResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<HighlightsResult>>> GetHighlights(
        [FromQuery] GetMonthlySummaryRequest request,
        [FromServices] IStatisticsUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            var userId = User.GetUserId();

            var result = await useCase.GetHighlightsAsync(
                request.Year,
                request.Month,
                userId);

            return Ok(ApiResponse<HighlightsResult>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponse<HighlightsResult>.Fail(
                    ApiStatus.Unauthorized,
                    ex.Message
                )
            );
        }
        catch (ArgumentException)
        {
            return BadRequest(
                ApiResponse<HighlightsResult>.Fail(ApiStatus.InvalidRequest)
            );
        }
        catch (Exception ex)
        {
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<HighlightsResult>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<HighlightsResult>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }

    /// <summary>
    /// 取引データの日付範囲を取得
    /// </summary>
    /// <param name="useCase">統計ユースケース</param>
    /// <param name="environment">ホスト環境</param>
    /// <returns>最古・最新の取引年月</returns>
    [HttpGet("date-range")]
    [SwaggerOperation(
        Summary = "取引データの日付範囲を取得",
        Description = "ユーザーの最古・最新の取引年月を取得します。\n\n" +
                    "取引が1件も存在しない場合、すべて null になります。")]
    [ProducesResponseType(typeof(ApiResponse<DateRangeResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DateRangeResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<DateRangeResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<DateRangeResult>>> GetDateRange(
        [FromServices] IStatisticsUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            var userId = User.GetUserId();

            var result = await useCase.GetDateRangeAsync(userId);

            return Ok(ApiResponse<DateRangeResult>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponse<DateRangeResult>.Fail(
                    ApiStatus.Unauthorized,
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
                    ApiResponse<DateRangeResult>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<DateRangeResult>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }
}
