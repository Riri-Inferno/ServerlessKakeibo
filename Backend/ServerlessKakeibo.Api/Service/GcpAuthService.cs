using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using ServerlessKakeibo.Api.Common.Settings;
using ServerlessKakeibo.Api.Service.Interface;
using System.Net;
using ServerlessKakeibo.Api.Common.Exceptions;

namespace ServerlessKakeibo.Api.Service;

/// <summary>
/// GCP認証サービス（ADC 一本化）
/// </summary>
public class GcpAuthService : IGcpAuthService
{
    private readonly GcpAuthSettings _options;
    private readonly ILogger<GcpAuthService> _logger;

    public GcpAuthService(
        IOptions<GcpAuthSettings> options,
        ILogger<GcpAuthService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 指定されたスコープでGoogleCredentialを取得
    /// </summary>
    public async Task<GoogleCredential> GetCredentialAsync(params string[] scopes)
    {
        try
        {
            var credential = await GoogleCredential.GetApplicationDefaultAsync();

            var targetScopes = (scopes != null && scopes.Length > 0)
                ? scopes
                : _options.DefaultScopes;

            if (credential.IsCreateScopedRequired && targetScopes.Length > 0)
            {
                credential = credential.CreateScoped(targetScopes);
            }

            return credential;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ADCからのCredential取得に失敗しました");
            throw new CustomException(
                new ExceptionType(
                    HttpStatusCode.InternalServerError,
                    ErrorMessages.Auth.AccessTokenFailed),
                ex);
        }
    }

    /// <summary>
    /// 指定されたスコープでアクセストークンを取得
    /// </summary>
    public async Task<string> GetAccessTokenAsync(params string[] scopes)
    {
        try
        {
            var credential = await GetCredentialAsync(scopes);

            if (credential is ITokenAccess tokenAccess)
            {
                var token = await tokenAccess.GetAccessTokenForRequestAsync();

                if (string.IsNullOrEmpty(token))
                {
                    throw new CustomException(
                        new ExceptionType(
                            HttpStatusCode.InternalServerError,
                            ErrorMessages.Auth.AccessTokenInvalid));
                }

                return token;
            }

            var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            return accessToken;
        }
        catch (CustomException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "アクセストークンの取得中にエラーが発生しました");
            throw new CustomException(
                new ExceptionType(
                    HttpStatusCode.InternalServerError,
                    ErrorMessages.Auth.AccessTokenFailed),
                ex);
        }
    }
}
