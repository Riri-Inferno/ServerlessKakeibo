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
    private readonly IGitHubLoginUseCase _gitHubLoginUseCase;
    private readonly IRefreshTokenUseCase _refreshTokenUseCase;
    private readonly ILogger<AuthController> _logger;
    private readonly IHostEnvironment _environment;

    public AuthController(
        IGoogleLoginUseCase googleLoginUseCase,
        IGitHubLoginUseCase gitHubLoginUseCase,
        IRefreshTokenUseCase refreshTokenUseCase,
        ILogger<AuthController> logger,
        IHostEnvironment environment)
    {
        _googleLoginUseCase = googleLoginUseCase ?? throw new ArgumentNullException(nameof(googleLoginUseCase));
        _gitHubLoginUseCase = gitHubLoginUseCase ?? throw new ArgumentNullException(nameof(gitHubLoginUseCase));
        _refreshTokenUseCase = refreshTokenUseCase ?? throw new ArgumentNullException(nameof(refreshTokenUseCase));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Google認証でログイン
    /// </summary>
    /// <param name="request">Googleログインリクエスト</param>
    /// <returns>JWT accessTokenとユーザー情報</returns>
    [HttpPost("google")]
    [SwaggerOperation(
        Summary = "Google認証でログイン",
        Description = @"
## 使い方

1. フロントエンドでGoogleログインを実行し、**Google ID Token**を取得  
2. 取得した **ID Token** を `idToken` フィールドに設定してリクエスト  
3. 初回ログイン時は自動でユーザーが作成されます  
4. レスポンスの `accessToken` を保存し、以降のAPI呼び出しで使用してください  

開発中は以下から **Google ID Token** を発行できます  
[Google OAuth Playground](https://developers.google.com/oauthplayground/)

## 注意事項

- **Google ID Token** を使用してください（Access Tokenではありません）
- ID Token は `eyJ` で始まる長い文字列です
- 取得した `accessToken` は  
  `Authorization: Bearer {accessToken}`  
  ヘッダーに設定して他のAPIを呼び出してください
")]
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
    /// GitHub認証でログイン
    /// </summary>
    /// <param name="request">GitHubログインリクエスト</param>
    /// <returns>JWT accessTokenとユーザー情報</returns>
    [HttpPost("github")]
    [SwaggerOperation(
        Summary = "GitHub認証でログイン",
        Description = @"
## 使い方

1. フロントエンドでGitHubログインを実行し、**認証コード**を取得  
2. 取得した **code** を `code` フィールドに設定してリクエスト  
3. 初回ログイン時は自動でユーザーが作成されます  
4. レスポンスの `accessToken` を保存し、以降のAPI呼び出しで使用してください  

## GitHub OAuth フロー

1. ユーザーを `https://github.com/login/oauth/authorize?client_id={CLIENT_ID}` にリダイレクト  
2. ユーザーが認証を許可すると、コールバックURLに `code` パラメータ付きでリダイレクトされる  
3. その `code` をこのAPIに送信  

## 注意事項

- メールアドレスが公開されていない場合、確認済みメールアドレスが必要です  
- 取得した `accessToken` は  
  `Authorization: Bearer {accessToken}`  
  ヘッダーに設定して他のAPIを呼び出してください
")]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LoginResult>>> GitHubLoginAsync(
        [FromBody] GitHubLoginRequest request)
    {
        try
        {
            _logger.LogInformation("GitHub認証リクエストを受信しました");

            var result = await _gitHubLoginUseCase.ExecuteAsync(request);

            _logger.LogInformation(
                "GitHub認証に成功しました。UserId: {UserId}",
                result.UserId);

            return Ok(ApiResponse<LoginResult>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "GitHub認証に失敗しました");
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
                    ex.Message
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

    /// <summary>
    /// トークンを更新
    /// </summary>
    /// <param name="request">リフレッシュトークンリクエスト</param>
    /// <returns>新しいアクセストークンとリフレッシュトークン</returns>
    [HttpPost("refresh")]
    [SwaggerOperation(
        Summary = "トークンを更新",
        Description = @"
## 使い方

1. ログイン時に取得した `refreshToken` を送信  
2. 新しい `accessToken` と `refreshToken` を取得  
3. 古い `refreshToken` は無効になるため、新しいものを保存してください  

## 有効期限

- **AccessToken**: 15分  
- **RefreshToken**: 7日  

## エラー

- リフレッシュトークンが無効または期限切れの場合は 401 エラー  
- この場合、再度 Google ログインが必要です
")]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<LoginResult>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LoginResult>>> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("トークン更新リクエストを受信しました");

            var result = await _refreshTokenUseCase.ExecuteAsync(request.RefreshToken);

            _logger.LogInformation(
                "トークン更新に成功しました。UserId: {UserId}",
                result.UserId);

            return Ok(ApiResponse<LoginResult>.Success(result));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "トークン更新に失敗しました");
            return Unauthorized(
                ApiResponse<LoginResult>.Fail(
                    ApiStatus.Unauthorized,
                    "リフレッシュトークンが無効または期限切れです。再度ログインしてください。"
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "トークン更新中にエラーが発生しました");

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
}
