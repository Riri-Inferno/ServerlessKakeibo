using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.ApiKey.Dto;
using ServerlessKakeibo.Api.Application.ApiKey.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// APIキー管理コントローラー
/// 発行・一覧・失効はいずれも JWT 認証のユーザー本人のみが行える
/// </summary>
[ApiController]
[Route("api/api-keys")]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ApiKeyController : ControllerBase
{
    /// <summary>
    /// APIキーを発行する
    /// 平文のキーはこのレスポンスでしか返されないため、クライアントは保存責任を持つ
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "APIキーを発行",
        Description = "APIキーを発行する。\n\n" +
                      "- 平文のキー (`key`) はこのレスポンスでのみ返却される。後から再表示はできない。\n" +
                      "- MVP では `read` スコープのみ発行可能。\n" +
                      "- `expiresAt` を null にすると無期限（自己責任）")]
    [ProducesResponseType(typeof(ApiResponse<CreateApiKeyResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateApiKeyResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<CreateApiKeyResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<CreateApiKeyResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateApiKeyResult>>> CreateAsync(
        [FromBody] CreateApiKeyRequest request,
        [FromServices] ICreateApiKeyUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, request);
            return Ok(ApiResponse<CreateApiKeyResult>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponse<CreateApiKeyResult>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                ApiResponse<CreateApiKeyResult>.Fail(ApiStatus.InvalidRequest, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                ApiResponse<CreateApiKeyResult>.Fail(ApiStatus.InvalidRequest, ex.Message));
        }
        catch (Exception ex)
        {
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<CreateApiKeyResult>.Fail(ApiStatus.InternalError));
            }
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<CreateApiKeyResult>.Fail(ApiStatus.InternalError, ex.ToString()));
        }
    }

    /// <summary>
    /// 自分の APIキー一覧を取得
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "APIキー一覧を取得",
        Description = "ログイン中のユーザーが発行している APIキーの一覧を取得する。\n\n" +
                      "失効済み (`revokedAt != null`) も含めて返す。平文キーは返さない。")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ApiKeyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ApiKeyDto>>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ApiKeyDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ApiKeyDto>>>> ListAsync(
        [FromServices] IListApiKeysUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId);
            return Ok(ApiResponse<IReadOnlyList<ApiKeyDto>>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponse<IReadOnlyList<ApiKeyDto>>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (Exception ex)
        {
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<IReadOnlyList<ApiKeyDto>>.Fail(ApiStatus.InternalError));
            }
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<IReadOnlyList<ApiKeyDto>>.Fail(ApiStatus.InternalError, ex.ToString()));
        }
    }

    /// <summary>
    /// APIキーを失効させる（物理削除はしない）
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "APIキーを失効",
        Description = "指定した APIキーを失効させる。\n\n" +
                      "物理削除ではなく `revokedAt` を立てる。失効後の認証は 401 になる。")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> RevokeAsync(
        [FromRoute] Guid id,
        [FromServices] IRevokeApiKeyUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            var userId = User.GetUserId();
            await useCase.ExecuteAsync(userId, id);
            return Ok(ApiResponse<object>.Success(new { }));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(
                ApiResponse<object>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                ApiResponse<object>.Fail(ApiStatus.NotFound, ex.Message));
        }
        catch (Exception ex)
        {
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(ApiStatus.InternalError));
            }
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail(ApiStatus.InternalError, ex.ToString()));
        }
    }
}
