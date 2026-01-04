using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Options;
using ServerlessKakeibo.Api.Common.Settings;
using ServerlessKakeibo.Api.Service.Interface;
using System.Net;
using ServerlessKakeibo.Api.Common.Exceptions;

namespace ServerlessKakeibo.Api.Service;

/// <summary>
/// GCP認証サービス
/// </summary>
public class GcpAuthService : IGcpAuthService
{
    private readonly GcpAuthSettings _options;
    private readonly ILogger<GcpAuthService> _logger;
    private readonly object _lock = new();
    private GoogleCredential? _baseCredential;

    /// <summary>
    /// コンストラクタ
    /// </summary>
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
    /// <param name="scopes">要求するスコープ</param>
    /// <returns>GoogleCredential</returns>
    public Task<GoogleCredential> GetCredentialAsync(params string[] scopes)
    {
        // ServiceAccountKeyPathが設定されていない場合
        if (string.IsNullOrEmpty(_options.ServiceAccountKeyPath))
        {
            // ADC（Application Default Credentials）を試みる
            return GetCredentialFromADCAsync(scopes);
        }

        try
        {
            // ベースクレデンシャルの初回作成またはキャッシュから取得
            if (_baseCredential == null)
            {
                lock (_lock)
                {
                    if (_baseCredential == null)
                    {
                        _baseCredential = CreateBaseCredential();
                    }
                }
            }

            // スコープが指定されている場合は、スコープ付きのクレデンシャルを返す
            var targetScopes = (scopes != null && scopes.Length > 0)
                ? scopes
                : _options.DefaultScopes;

            var credential = (_baseCredential.IsCreateScopedRequired && targetScopes.Length > 0)
                ? _baseCredential.CreateScoped(targetScopes)
                : _baseCredential;

            return Task.FromResult(credential);
        }
        catch (Exception ex) when (ex is not CustomException)
        {
            _logger.LogError(ex, "GoogleCredentialの取得中にエラーが発生しました");
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
    /// <param name="scopes">要求するスコープ</param>
    /// <returns>アクセストークン</returns>
    public async Task<string> GetAccessTokenAsync(params string[] scopes)
    {
        try
        {
            var credential = await GetCredentialAsync(scopes);

            // ITokenAccessインターフェースを使用（推奨）
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

            // 互換性のためのフォールバック
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

    /// <summary>
    /// ベースとなるGoogleCredentialを作成
    /// </summary>
    private GoogleCredential CreateBaseCredential()
    {
        // ServiceAccountKeyPathから作成
        if (string.IsNullOrEmpty(_options.ServiceAccountKeyPath))
        {
            throw new CustomException(
                new ExceptionType(
                    HttpStatusCode.InternalServerError,
                    ErrorMessages.Auth.MissingServiceAccountJson));
        }

        using var stream = new FileStream(
            _options.ServiceAccountKeyPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: false);

        var serviceAccountCredential = ServiceAccountCredential
            .FromServiceAccountData(stream);

        return serviceAccountCredential.ToGoogleCredential();
    }

    /// <summary>
    /// ADC (Application Default Credentials) からCredentialを取得
    /// </summary>
    private async Task<GoogleCredential> GetCredentialFromADCAsync(string[] scopes)
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
}
