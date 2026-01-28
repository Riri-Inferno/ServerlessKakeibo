using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.UserData.Dto;
using ServerlessKakeibo.Api.Application.UserData.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers
{
    /// <summary>
    /// ユーザーデータ管理コントローラー
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserDataController : ControllerBase
    {
        private readonly ILogger<UserDataController> _logger;

        public UserDataController(ILogger<UserDataController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// ユーザーの全取引データを削除
        /// </summary>
        [HttpDelete("transactions")]
        [SwaggerOperation(
            Summary = "全取引データを削除",
            Description = "ログインユーザーに紐づく全ての取引データを論理削除します。\n\n" +
                          "削除対象:\n" +
                          "- 取引本体\n" +
                          "- 取引明細\n" +
                          "- 店舗詳細\n" +
                          "- 税詳細\n" +
                          "- レシート画像（GCS）\n\n" +
                          "**この操作は取り消せません。**")]
        [ProducesResponseType(typeof(ApiResponse<DeleteAllTransactionsResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DeleteAllTransactionsResult>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<DeleteAllTransactionsResult>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<DeleteAllTransactionsResult>>> DeleteAllTransactionsAsync(
            [FromServices] IDeleteAllTransactionsUseCase useCase,
            [FromServices] IHostEnvironment environment,
            CancellationToken cancellationToken)
        {
            try
            {
                // JWTからユーザーIDを取得
                var userId = User.GetUserId();

                _logger.LogInformation(
                    "全取引削除リクエスト受信。UserId: {UserId}",
                    userId);

                var result = await useCase.ExecuteAsync(userId, cancellationToken);

                return Ok(ApiResponse<DeleteAllTransactionsResult>.Success(result));
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "認証エラー");

                return Unauthorized(
                    ApiResponse<DeleteAllTransactionsResult>.Fail(
                        ApiStatus.Unauthorized,
                        ex.Message
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "全取引削除中にエラーが発生しました");

                if (!environment.IsDevelopment())
                {
                    return StatusCode(
                        StatusCodes.Status500InternalServerError,
                        ApiResponse<DeleteAllTransactionsResult>.Fail(ApiStatus.InternalError)
                    );
                }

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<DeleteAllTransactionsResult>.Fail(
                        ApiStatus.InternalError,
                        ex.ToString()
                    )
                );
            }
        }
    }
}
