using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.ItemCategory.Dto;
using ServerlessKakeibo.Api.Application.ItemCategory.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// 商品カテゴリ管理コントローラー
/// </summary>
[ApiController]
[Route("api/categories/item")]
[Authorize]
public class ItemCategoryController : ControllerBase
{
    private readonly ILogger<ItemCategoryController> _logger;

    public ItemCategoryController(ILogger<ItemCategoryController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 商品カテゴリ一覧を取得
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "商品カテゴリ一覧取得",
        Description = "ログインユーザーの商品カテゴリ一覧（支出用）を取得します。")]
    [ProducesResponseType(typeof(ApiResponse<ItemCategoryListResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ItemCategoryListResult>>> GetCategoriesAsync(
        [FromServices] IGetItemCategoriesUseCase useCase,
        [FromQuery] bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, includeHidden, cancellationToken);
            return Ok(ApiResponse<ItemCategoryListResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品カテゴリ一覧取得中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<ItemCategoryListResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// カスタム商品カテゴリを作成
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "カスタムカテゴリ作成")]
    [ProducesResponseType(typeof(ApiResponse<ItemCategoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ItemCategoryResult>>> CreateCategoryAsync(
        [FromServices] ICreateItemCategoryUseCase useCase,
        [FromBody] CreateItemCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, request, cancellationToken);
            return Ok(ApiResponse<ItemCategoryResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ作成中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<ItemCategoryResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 商品カテゴリを更新
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "カテゴリ更新")]
    [ProducesResponseType(typeof(ApiResponse<ItemCategoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ItemCategoryResult>>> UpdateCategoryAsync(
        [FromRoute] Guid id,
        [FromServices] IUpdateItemCategoryUseCase useCase,
        [FromBody] UpdateItemCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, id, request, cancellationToken);
            return Ok(ApiResponse<ItemCategoryResult>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ItemCategoryResult>.Fail(ApiStatus.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<ItemCategoryResult>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ更新中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<ItemCategoryResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 商品カテゴリを削除（非表示化）
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "カテゴリ削除（非表示化）")]
    [ProducesResponseType(typeof(ApiResponse<ItemCategoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ItemCategoryResult>>> DeleteCategoryAsync(
        [FromRoute] Guid id,
        [FromServices] IDeleteItemCategoryUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, id, cancellationToken);
            return Ok(ApiResponse<ItemCategoryResult>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ItemCategoryResult>.Fail(ApiStatus.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<ItemCategoryResult>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ削除中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<ItemCategoryResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 商品カテゴリを復元
    /// </summary>
    [HttpPost("{id}/restore")]
    [SwaggerOperation(Summary = "カテゴリ復元")]
    [ProducesResponseType(typeof(ApiResponse<ItemCategoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ItemCategoryResult>>> RestoreCategoryAsync(
        [FromRoute] Guid id,
        [FromServices] IRestoreItemCategoryUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, id, cancellationToken);
            return Ok(ApiResponse<ItemCategoryResult>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ItemCategoryResult>.Fail(ApiStatus.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<ItemCategoryResult>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ復元中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<ItemCategoryResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 商品カテゴリをマスタ設定に戻す
    /// </summary>
    [HttpPost("reset")]
    [SwaggerOperation(
        Summary = "マスタ設定に戻す",
        Description = "マスタ由来のカテゴリを削除し、初期状態に戻します。カスタムカテゴリは保持されます。")]
    [ProducesResponseType(typeof(ApiResponse<ItemCategoryListResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ItemCategoryListResult>>> ResetToMasterAsync(
        [FromServices] IResetItemCategoriesToMasterUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, cancellationToken);
            return Ok(ApiResponse<ItemCategoryListResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "マスタ設定リセット中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<ItemCategoryListResult>.Fail(ApiStatus.InternalError));
        }
    }
}
