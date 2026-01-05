using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.ResistReceiptDetails.Dto;
using ServerlessKakeibo.Api.Application.ResistReceiptDetails.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ResistReceiptDetailsController : ControllerBase
{
    private readonly ILogger<ResistReceiptDetailsController> _logger;

    public ResistReceiptDetailsController(ILogger<ResistReceiptDetailsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 領収書解析結果を取引として保存する
    /// </summary>
    [HttpPost("save-transaction")]
    [SwaggerOperation(
        Summary = "領収書解析結果を取引として保存",
        Description = "領収書解析APIの結果を受け取り、データベースに取引として保存する")]
    public async Task<ActionResult<ApiResponse<SaveTransactionResultDto>>> SaveTransactionAsync(
        [FromBody] ResistReceiptDetailsRequest request,
        [FromServices] IResistReceiptDetailsUseCase useCase,
        [FromServices] IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        // リクエスト検証
        if (request?.ParseResult == null)
        {
            return BadRequest(
                ApiResponse<SaveTransactionResultDto>.Fail(
                    ApiStatus.InvalidRequest,
                    "領収書解析結果が指定されていません"
                )
            );
        }

        try
        {
            // TODO: 認証実装後はログインユーザーIDを取得
            var userId = Guid.Parse("a1111111-1111-1111-1111-111111111111"); // 仮のユーザーID

            var result = await useCase.ExecuteSaveAsync(
                request.ParseResult,
                userId,
                cancellationToken);

            // 詳細情報を含むレスポンスを返す
            return Ok(ApiResponse<SaveTransactionResultDto>.Success(result));
        }
        catch (ArgumentException argEx)
        {
            _logger.LogWarning(argEx, "不正なリクエストパラメータ");
            return BadRequest(
                ApiResponse<SaveTransactionResultDto>.Fail(
                    ApiStatus.InvalidRequest,
                    argEx.Message
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取引保存中にエラーが発生しました");

            // 開発環境以外では詳細を返さない
            if (!environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<SaveTransactionResultDto>.Fail(
                        ApiStatus.InternalError,
                        "取引の保存に失敗しました"
                    )
                );
            }

            // 開発環境では詳細を返す
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<SaveTransactionResultDto>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }
}
