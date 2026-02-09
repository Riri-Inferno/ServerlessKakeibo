using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;
using ServerlessKakeibo.Api.Application.IncomeItemCategory.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// 給与項目カテゴリ管理コントローラー
/// </summary>
[ApiController]
[Route("api/categories/income-item")]
[Authorize]
public class IncomeItemCategoryController : ControllerBase
{
    private readonly ILogger<IncomeItemCategoryController> _logger;

    public IncomeItemCategoryController(ILogger<IncomeItemCategoryController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 給与項目カテゴリ一覧を取得
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "給与項目カテゴリ一覧取得",
        Description = "ログインユーザーの給与項目カテゴリ一覧（収入用）を取得します。")]
    [ProducesResponseType(typeof(ApiResponse<IncomeItemCategoryListResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IncomeItemCategoryListResult>>> GetCategoriesAsync(
        [FromServices] IGetIncomeItemCategoriesUseCase useCase,
        [FromQuery] bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, includeHidden, cancellationToken);
            return Ok(ApiResponse<IncomeItemCategoryListResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "給与項目カテゴリ一覧取得中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IncomeItemCategoryListResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// カスタム給与項目カテゴリを作成
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "カスタムカテゴリ作成")]
    [ProducesResponseType(typeof(ApiResponse<IncomeItemCategoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IncomeItemCategoryResult>>> CreateCategoryAsync(
        [FromServices] ICreateIncomeItemCategoryUseCase useCase,
        [FromBody] CreateIncomeItemCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, request, cancellationToken);
            return Ok(ApiResponse<IncomeItemCategoryResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ作成中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IncomeItemCategoryResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 給与項目カテゴリを更新
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "カテゴリ更新")]
    [ProducesResponseType(typeof(ApiResponse<IncomeItemCategoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IncomeItemCategoryResult>>> UpdateCategoryAsync(
        [FromRoute] Guid id,
        [FromServices] IUpdateIncomeItemCategoryUseCase useCase,
        [FromBody] UpdateIncomeItemCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, id, request, cancellationToken);
            return Ok(ApiResponse<IncomeItemCategoryResult>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IncomeItemCategoryResult>.Fail(ApiStatus.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<IncomeItemCategoryResult>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ更新中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IncomeItemCategoryResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 給与項目カテゴリを削除（非表示化）
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "カテゴリ削除（非表示化）")]
    [ProducesResponseType(typeof(ApiResponse<IncomeItemCategoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IncomeItemCategoryResult>>> DeleteCategoryAsync(
        [FromRoute] Guid id,
        [FromServices] IDeleteIncomeItemCategoryUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, id, cancellationToken);
            return Ok(ApiResponse<IncomeItemCategoryResult>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IncomeItemCategoryResult>.Fail(ApiStatus.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<IncomeItemCategoryResult>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ削除中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IncomeItemCategoryResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 給与項目カテゴリを復元
    /// </summary>
    [HttpPost("{id}/restore")]
    [SwaggerOperation(Summary = "カテゴリ復元")]
    [ProducesResponseType(typeof(ApiResponse<IncomeItemCategoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IncomeItemCategoryResult>>> RestoreCategoryAsync(
        [FromRoute] Guid id,
        [FromServices] IRestoreIncomeItemCategoryUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, id, cancellationToken);
            return Ok(ApiResponse<IncomeItemCategoryResult>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IncomeItemCategoryResult>.Fail(ApiStatus.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<IncomeItemCategoryResult>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ復元中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IncomeItemCategoryResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 給与項目カテゴリをマスタ設定に戻す
    /// </summary>
    [HttpPost("reset")]
    [SwaggerOperation(
        Summary = "マスタ設定に戻す",
        Description = "マスタ由来のカテゴリを削除し、初期状態に戻します。カスタムカテゴリは保持されます。")]
    [ProducesResponseType(typeof(ApiResponse<IncomeItemCategoryListResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IncomeItemCategoryListResult>>> ResetToMasterAsync(
        [FromServices] IResetIncomeItemCategoriesToMasterUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, cancellationToken);
            return Ok(ApiResponse<IncomeItemCategoryListResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "マスタ設定リセット中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IncomeItemCategoryListResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 給与項目カテゴリの並び順を一括更新
    /// </summary>
    [HttpPut("order")]
    [SwaggerOperation(
        Summary = "並び順一括更新",
        Description = "複数の給与項目カテゴリの表示順序を一括で更新します。")]
    [ProducesResponseType(typeof(ApiResponse<IncomeItemCategoryListResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IncomeItemCategoryListResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<IncomeItemCategoryListResult>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IncomeItemCategoryListResult>>> UpdateCategoryOrderAsync(
        [FromServices] IUpdateIncomeItemCategoryOrderUseCase useCase,
        [FromBody] UpdateIncomeItemCategoryOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, request, cancellationToken);
            return Ok(ApiResponse<IncomeItemCategoryListResult>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<IncomeItemCategoryListResult>.Fail(ApiStatus.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<IncomeItemCategoryListResult>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "並び順更新中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IncomeItemCategoryListResult>.Fail(ApiStatus.InternalError));
        }
    }
}
