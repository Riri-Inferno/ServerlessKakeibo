using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.TransactionQuery.Dto;
using ServerlessKakeibo.Api.Application.TransactionQuery.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using Swashbuckle.AspNetCore.Annotations;
using ServerlessKakeibo.Api.Common.Models;
using ServerlessKakeibo.Api.Application.Transaction.Dto;
using ServerlessKakeibo.Api.Application.Transaction.Usecases;
using System.Security.Cryptography.X509Certificates;

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

    /// <summary>
    /// 取引一覧を取得
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "取引一覧を取得",
        Description = "取引の一覧をページングして取得する。\n\n日付範囲、カテゴリ、金額などでフィルタ可能。")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TransactionSummaryResult>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TransactionSummaryResult>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TransactionSummaryResult>>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<TransactionSummaryResult>>>> GetTransactionsAsync(
        [FromQuery] GetTransactionsRequest request,
        [FromServices] ITransactionQueryUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            // TODO: 認証実装後はトークンからユーザーIDを取得
            var userId = Guid.Parse("a1111111-1111-1111-1111-111111111111");

            var result = await useCase.GetPagedListAsync(request, userId);

            return Ok(ApiResponse<PagedResult<TransactionSummaryResult>>.Success(result));
        }
        catch (ArgumentException)
        {
            return BadRequest(
                ApiResponse<PagedResult<TransactionSummaryResult>>.Fail(ApiStatus.InvalidRequest)
            );
        }
        catch (Exception ex)
        {
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<PagedResult<TransactionSummaryResult>>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<PagedResult<TransactionSummaryResult>>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }

    /// <summary>
    /// 取引を新規作成
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "取引を新規作成",
        Description = "新しい取引を作成する。\n\n" +
                      "金額はクライアント指定値を優先します。")]
    [ProducesResponseType(typeof(ApiResponse<TransactionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransactionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TransactionResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TransactionResult>>> CreateTransactionAsync(
        [FromBody] CreateTransactionRequest request,
        [FromServices] ITransactionCreateUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            // TODO: 認証実装後はトークンからユーザーIDを取得
            var userId = Guid.Parse("a1111111-1111-1111-1111-111111111111");

            var result = await useCase.ExecuteAsync(request, userId);

            return Ok(ApiResponse<TransactionResult>.Success(result));
        }
        catch (ArgumentException)
        {
            return BadRequest(
                ApiResponse<TransactionResult>.Fail(ApiStatus.InvalidRequest)
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                ApiResponse<TransactionResult>.Fail(
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
                    ApiResponse<TransactionResult>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<TransactionResult>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }

    /// <summary>
    /// 取引を更新
    /// </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "取引を更新",
        Description = "既存の取引を更新する。\n\n" +
                      "金額はサーバー側で自動計算されます（Items合計 + Taxes合計）。\n" +
                      "子エンティティ（Items, Taxes, ShopDetails）は Full Replace 方式です。")]
    [ProducesResponseType(typeof(ApiResponse<TransactionResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransactionResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TransactionResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<TransactionResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TransactionResult>>> UpdateTransactionAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateTransactionRequest request,
        [FromServices] ITransactionUpdateUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            // TODO: 認証実装後はトークンからユーザーIDを取得
            var userId = Guid.Parse("a1111111-1111-1111-1111-111111111111");

            var result = await useCase.ExecuteAsync(id, request, userId);

            return Ok(ApiResponse<TransactionResult>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                ApiResponse<TransactionResult>.Fail(
                    ApiStatus.NotFound,
                    ex.Message
                )
            );
        }
        catch (ArgumentException)
        {
            return BadRequest(
                ApiResponse<TransactionResult>.Fail(ApiStatus.InvalidRequest)
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                ApiResponse<TransactionResult>.Fail(
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
                    ApiResponse<TransactionResult>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<TransactionResult>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }

    /// <summary>
    /// 取引を削除
    /// </summary>
    /// <param name="id"></param>
    /// <param name="useCase"></param>
    /// <param name="environment"></param>
    /// <returns></returns>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "取引を削除",
        Description = "既存の取引を論理削除する。")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDeleteResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransactionDeleteResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<TransactionDeleteResult>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<TransactionDeleteResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<TransactionDeleteResult>>> DeleteTransactionAsync(
        [FromRoute] Guid id,
        [FromServices] ITransactionDeleteUseCase useCase,
        [FromServices] IHostEnvironment environment)
    {
        try
        {
            // TODO: 認証実装後はトークンからユーザーIDを取得
            var userId = Guid.Parse("a1111111-1111-1111-1111-111111111111");

            var result = await useCase.ExecuteAsync(id, userId);

            return Ok(ApiResponse<TransactionDeleteResult>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(
                            ApiResponse<TransactionDeleteResult>.Fail(
                                ApiStatus.NotFound,
                                ex.Message
                            )
                        );
        }
        catch (ArgumentException)
        {
            return BadRequest(
                ApiResponse<TransactionDeleteResult>.Fail(ApiStatus.InvalidRequest)
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                ApiResponse<TransactionDeleteResult>.Fail(
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
                    ApiResponse<TransactionDeleteResult>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<TransactionDeleteResult>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }
}
