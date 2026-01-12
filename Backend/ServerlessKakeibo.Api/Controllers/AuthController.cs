using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ServerlessKakeibo.Api.Application.Authentication.Dto;
using ServerlessKakeibo.Api.Application.Authentication.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Contracts.Enums;
using ServerlessKakeibo.Api.Controllers.Extensions;

namespace ServerlessKakeibo.Api.Controllers;

/// <summary>
/// 認証コントローラー
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IGoogleLoginUseCase _googleLoginUseCase;
    private readonly ILogger<AuthController> _logger;
    private readonly IHostEnvironment _environment;

    public AuthController(
        IGoogleLoginUseCase googleLoginUseCase,
        ILogger<AuthController> logger,
        IHostEnvironment environment)
    {
        _googleLoginUseCase = googleLoginUseCase ?? throw new ArgumentNullException(nameof(googleLoginUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Google認証でログイン
    /// </summary>
    [HttpPost("google")]
    [SwaggerOperation(
        Summary = "Google認証でログイン",
        Description = "GoogleのIDトークンを使用してログインし、JWTトークンを発行する。")]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LoginResult>>> GoogleLoginAsync(
        [FromBody] GoogleLoginRequest request)
    {
        try
        {
            _logger.LogInformation("Google認証リクエストを受信しました");

            var result = await _googleLoginUseCase.ExecuteAsync(request.IdToken);

            _logger.LogInformation(
                "Google認証に成功しました。UserId: {UserId}",
                result.UserId);

            return Ok(ApiResponse<LoginResult>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Google認証に失敗しました");
            return Unauthorized(
                ApiResponse<LoginResult>.Fail(
                    ApiStatus.Unauthorized,
                    "認証に失敗しました。"
                )
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "不正なリクエストです");
            return BadRequest(
                ApiResponse<LoginResult>.Fail(
                    ApiStatus.InvalidRequest,
                    ex.Message
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "認証処理中にエラーが発生しました");
            return BadRequest(
                ApiResponse<LoginResult>.Fail(
                    ApiStatus.InvalidRequest,
                    "認証処理に失敗しました。"
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "予期しないエラーが発生しました");

            if (!_environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<LoginResult>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<LoginResult>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }

    /// <summary>
    /// 現在のユーザー情報を取得
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(
        Summary = "現在のユーザー情報を取得",
        Description = "JWTトークンから現在ログイン中のユーザー情報を取得する。")]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<CurrentUserResponse>), StatusCodes.Status500InternalServerError)]
    public ActionResult<ApiResponse<CurrentUserResponse>> GetCurrentUser()
    {
        try
        {
            var userId = User.GetUserId();
            var displayName = User.GetDisplayName();
            var email = User.GetEmail();

            _logger.LogDebug("ユーザー情報を取得しました。UserId: {UserId}", userId);

            var response = new CurrentUserResponse(
                userId,
                displayName,
                email
            );

            return Ok(ApiResponse<CurrentUserResponse>.Success(response));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "認証情報が不正です");
            return Unauthorized(
                ApiResponse<CurrentUserResponse>.Fail(
                    ApiStatus.Unauthorized,
                    "認証情報が不正です。"
                )
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ユーザー情報取得中にエラーが発生しました");

            if (!_environment.IsDevelopment())
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<CurrentUserResponse>.Fail(ApiStatus.InternalError)
                );
            }

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                ApiResponse<CurrentUserResponse>.Fail(
                    ApiStatus.InternalError,
                    ex.ToString()
                )
            );
        }
    }
}
