using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.UserSettings.Dto;
using ServerlessKakeibo.Api.Application.UserSettings.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// ユーザー設定コントローラー
/// </summary>
[ApiController]
[Route("api/user/settings")]
[Authorize]
public class UserSettingsController : ControllerBase
{
    /// <summary>
    /// ユーザー設定を取得
    /// </summary>
    /// <param name="useCase">ユーザー設定取得ユースケース</param>
    /// <param name="environment">ホスト環境</param>
    /// <returns>ユーザー設定</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "ユーザー設定を取得",
        Description = "現在のユーザーの設定情報を取得する。\n\n" +
                      "表示名、締め日、タイムゾーン、通貨などの個人設定を含む。")]
    [ProducesResponseType(typeof(ApiResponse<UserSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserSettingsDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserSettingsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserSettingsDto>>> GetSettingsAsync(
        [FromServices] IGetUserSettingsUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            var userId = User.GetUserId();

            var result = await useCase.ExecuteAsync(userId);

            return Ok(ApiResponse<UserSettingsDto>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponse<UserSettingsDto>.Fail(
                    ApiStatus.Unauthorized,
                    ex.Message
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(
                ApiResponse<UserSettingsDto>.Fail(
                    ApiStatus.NotFound,
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
                    ApiResponse<UserSettingsDto>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<UserSettingsDto>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }

    /// <summary>
    /// ユーザー設定を更新
    /// </summary>
    /// <param name="request">更新リクエスト</param>
    /// <param name="useCase">ユーザー設定更新ユースケース</param>
    /// <param name="environment">ホスト環境</param>
    /// <returns>更新後のユーザー設定</returns>
    [HttpPut]
    [SwaggerOperation(
        Summary = "ユーザー設定を更新",
        Description = "ユーザー設定を更新する。\n\n" +
                      "- DisplayNameOverride: 空文字列を指定するとGoogle情報に戻る\n" +
                      "- ClosingDay: 1-31の範囲で指定、nullは月末締め\n" +
                      "- TimeZone: IANA形式(例: Asia/Tokyo)\n" +
                      "- CurrencyCode: ISO 4217形式(例: JPY)")]
    [ProducesResponseType(typeof(ApiResponse<UserSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserSettingsDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UserSettingsDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserSettingsDto>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserSettingsDto>>> UpdateSettingsAsync(
        [FromBody] UpdateUserSettingsRequest request,
        [FromServices] IUpdateUserSettingsUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            var userId = User.GetUserId();

            var result = await useCase.ExecuteAsync(userId, request);

            if (!result.Success)
            {
                return BadRequest(
                    ApiResponse<UserSettingsDto>.Fail(
                        ApiStatus.InvalidRequest,
                        result.ErrorMessage ?? "設定の更新に失敗しました"
                    )
                );
            }

            return Ok(ApiResponse<UserSettingsDto>.Success(result.Settings!));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponse<UserSettingsDto>.Fail(
                    ApiStatus.Unauthorized,
                    ex.Message
                )
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ApiResponse<UserSettingsDto>.Fail(
                    ApiStatus.InvalidRequest,
                    ex.Message
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                ApiResponse<UserSettingsDto>.Fail(
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
                    ApiResponse<UserSettingsDto>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<UserSettingsDto>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }
}
