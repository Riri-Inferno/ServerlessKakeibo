using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;
using ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// 取引カテゴリ管理コントローラー
/// </summary>
[ApiController]
[Route("api/categories/transaction")]
[Authorize]
public class TransactionCategoryController : ControllerBase
{
    private readonly ILogger<TransactionCategoryController> _logger;

    public TransactionCategoryController(ILogger<TransactionCategoryController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 取引カテゴリ一覧を取得
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "取引カテゴリ一覧取得",
        Description = "ログインユーザーの取引カテゴリ一覧を取得します。\n\n" +
                      "- マスタ由来のカテゴリ\n" +
                      "- ユーザー追加のカスタムカテゴリ\n" +
                      "- includeHidden=true で非表示カテゴリも取得")]
    [ProducesResponseType(typeof(ApiResponse<TransactionCategoryListResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TransactionCategoryListResult>>> GetCategoriesAsync(
        [FromServices] IGetTransactionCategoriesUseCase useCase,
        [FromQuery] bool includeHidden = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, includeHidden, cancellationToken);
            return Ok(ApiResponse<TransactionCategoryListResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取引カテゴリ一覧取得中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<TransactionCategoryListResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// カスタム取引カテゴリを作成
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "カスタムカテゴリ作成",
        Description = "ユーザー独自の取引カテゴリを作成します。")]
    [ProducesResponseType(typeof(ApiResponse<TransactionCategoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TransactionCategoryResult>>> CreateCategoryAsync(
        [FromServices] ICreateTransactionCategoryUseCase useCase,
        [FromBody] CreateTransactionCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, request, cancellationToken);
            return Ok(ApiResponse<TransactionCategoryResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ作成中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<TransactionCategoryResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 取引カテゴリを更新
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "カテゴリ更新",
        Description = "カテゴリの名前・色・表示順序を更新します。")]
    [ProducesResponseType(typeof(ApiResponse<TransactionCategoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TransactionCategoryResult>>> UpdateCategoryAsync(
        [FromRoute] Guid id,
        [FromServices] IUpdateTransactionCategoryUseCase useCase,
        [FromBody] UpdateTransactionCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, id, request, cancellationToken);
            return Ok(ApiResponse<TransactionCategoryResult>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TransactionCategoryResult>.Fail(ApiStatus.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<TransactionCategoryResult>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ更新中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<TransactionCategoryResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 取引カテゴリを削除（非表示化）
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "カテゴリ削除（非表示化）",
        Description = "カテゴリを非表示にします。復元可能です。")]
    [ProducesResponseType(typeof(ApiResponse<TransactionCategoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TransactionCategoryResult>>> DeleteCategoryAsync(
        [FromRoute] Guid id,
        [FromServices] IDeleteTransactionCategoryUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, id, cancellationToken);
            return Ok(ApiResponse<TransactionCategoryResult>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TransactionCategoryResult>.Fail(ApiStatus.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<TransactionCategoryResult>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ削除中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<TransactionCategoryResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 取引カテゴリを復元
    /// </summary>
    [HttpPost("{id}/restore")]
    [SwaggerOperation(
        Summary = "カテゴリ復元",
        Description = "非表示にしたカテゴリを再表示します。")]
    [ProducesResponseType(typeof(ApiResponse<TransactionCategoryResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TransactionCategoryResult>>> RestoreCategoryAsync(
        [FromRoute] Guid id,
        [FromServices] IRestoreTransactionCategoryUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, id, cancellationToken);
            return Ok(ApiResponse<TransactionCategoryResult>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TransactionCategoryResult>.Fail(ApiStatus.NotFound, ex.Message));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<TransactionCategoryResult>.Fail(ApiStatus.Unauthorized, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "カテゴリ復元中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<TransactionCategoryResult>.Fail(ApiStatus.InternalError));
        }
    }

    /// <summary>
    /// 取引カテゴリをマスタ設定に戻す
    /// </summary>
    [HttpPost("reset")]
    [SwaggerOperation(
        Summary = "マスタ設定に戻す",
        Description = "マスタ由来のカテゴリを削除し、初期状態に戻します。\n\n" +
                      "**カスタムカテゴリは保持されます。**")]
    [ProducesResponseType(typeof(ApiResponse<TransactionCategoryListResult>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<TransactionCategoryListResult>>> ResetToMasterAsync(
        [FromServices] IResetTransactionCategoriesToMasterUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.GetUserId();
            var result = await useCase.ExecuteAsync(userId, cancellationToken);
            return Ok(ApiResponse<TransactionCategoryListResult>.Success(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "マスタ設定リセット中にエラーが発生しました");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<TransactionCategoryListResult>.Fail(ApiStatus.InternalError));
        }
    }
}
