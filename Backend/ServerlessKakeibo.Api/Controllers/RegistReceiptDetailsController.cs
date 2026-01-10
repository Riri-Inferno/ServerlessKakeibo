using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.registReceiptDetails.Usecases;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Controllers;

[ApiController]
[Route("api/receipt")]
public class registReceiptDetailsController : ControllerBase
{
    private readonly ILogger<registReceiptDetailsController> _logger;

    public registReceiptDetailsController(
        ILogger<registReceiptDetailsController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 領収書解析結果を保存
    /// </summary>
    [HttpPost("parse-and-save")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SaveTransactionAsync(
        [FromBody] SaveReceiptParseResultRequest request,
        [FromServices] IregistReceiptDetailsUseCase useCase,
        [FromServices] IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        try
        {
            // TODO: 認証実装後はトークンからユーザーIDを取得
            var userId = Guid.Parse("a1111111-1111-1111-1111-111111111111");

            var result = await useCase.ExecuteSaveAsync(request, userId, cancellationToken);

            return Ok(new
            {
                status = "Success",
                message = (string?)null,
                data = result
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "リクエストが不正です");
            return BadRequest(new
            {
                status = "BadRequest",
                message = ex.Message,
                data = (object?)null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "取引保存に失敗しました");
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                status = "InternalError",
                message = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "予期しないエラーが発生しました");

            var message = environment.IsDevelopment()
                ? ex.ToString()
                : "取引保存中にエラーが発生しました";

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                status = "InternalError",
                message,
                data = (object?)null
            });
        }
    }
}
