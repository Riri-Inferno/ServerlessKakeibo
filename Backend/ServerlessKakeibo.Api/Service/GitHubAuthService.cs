using System.Net.Http.Headers;
using System.Text.Json;
using ServerlessKakeibo.Api.Service.Interface;
using ServerlessKakeibo.Api.Service.Models;

namespace ServerlessKakeibo.Api.Service;

/// <summary>
/// GitHub認証サービス実装
/// </summary>
public class GitHubAuthService : IGitHubAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GitHubAuthService> _logger;

    public GitHubAuthService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GitHubAuthService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 認証コードからユーザー情報を取得
    /// </summary>
    public async Task<GitHubUserInfo> GetUserInfoAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("認証コードが指定されていません", nameof(code));

        try
        {
            _logger.LogInformation("GitHub認証を開始します");

            // 1. アクセストークンを取得
            var accessToken = await ExchangeCodeForTokenAsync(code, cancellationToken);

            // 2. ユーザー情報を取得
            var userInfo = await FetchUserInfoAsync(accessToken, cancellationToken);

            // 3. メールアドレスが取得できない場合は別APIで取得
            if (string.IsNullOrEmpty(userInfo.Email))
            {
                var primaryEmail = await FetchPrimaryEmailAsync(accessToken, cancellationToken);

                if (string.IsNullOrEmpty(primaryEmail))
                {
                    _logger.LogError("GitHubからメールアドレスを取得できませんでした。Login: {Login}", userInfo.Login);
                    throw new InvalidOperationException("GitHubアカウントに確認済みメールアドレスが設定されていません。");
                }

                userInfo = userInfo with { Email = primaryEmail };
            }

            _logger.LogInformation(
                "GitHub認証に成功しました。Login: {Login}, Email: {Email}",
                userInfo.Login, userInfo.Email);

            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GitHub認証中にエラーが発生しました");
            throw new InvalidOperationException("GitHub認証に失敗しました", ex);
        }
    }

    #region Private Methods

    /// <summary>
    /// 認証コードをアクセストークンに交換
    /// </summary>
    private async Task<string> ExchangeCodeForTokenAsync(string code, CancellationToken cancellationToken)
    {
        var clientId = _configuration["Authentication:GitHub:ClientId"];
        var clientSecret = _configuration["Authentication:GitHub:ClientSecret"];

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            _logger.LogError("GitHub ClientId/ClientSecretが設定されていません");
            throw new InvalidOperationException("GitHub認証の設定が不正です");
        }

        var request = new HttpRequestMessage(HttpMethod.Post, "https://github.com/login/oauth/access_token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
                ["code"] = code
            })
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var json = JsonDocument.Parse(content);

        if (json.RootElement.TryGetProperty("error", out var error))
        {
            var errorDescription = json.RootElement.TryGetProperty("error_description", out var desc)
                ? desc.GetString()
                : "不明なエラー";

            _logger.LogWarning("GitHubトークン取得エラー: {Error} - {Description}", error.GetString(), errorDescription);
            throw new InvalidOperationException($"GitHubトークンの取得に失敗しました: {errorDescription}");
        }

        return json.RootElement.GetProperty("access_token").GetString()
            ?? throw new InvalidOperationException("アクセストークンの取得に失敗しました");
    }

    /// <summary>
    /// アクセストークンからユーザー情報を取得
    /// </summary>
    private async Task<GitHubUserInfo> FetchUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("ServerlessKakeibo", "1.0"));

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var json = JsonDocument.Parse(content);
        var root = json.RootElement;

        // ID（必須）
        var id = root.GetProperty("id").GetInt64().ToString();

        // Login（必須）
        var login = root.GetProperty("login").GetString()
            ?? throw new InvalidOperationException("GitHubのログイン名を取得できませんでした");

        // Name（任意）
        var name = root.TryGetProperty("name", out var nameElement) && nameElement.ValueKind != JsonValueKind.Null
            ? nameElement.GetString()
            : null;

        // Email（任意 - 公開されている場合のみ）
        var email = root.TryGetProperty("email", out var emailElement) && emailElement.ValueKind != JsonValueKind.Null
            ? emailElement.GetString()
            : null;

        // AvatarUrl（任意）
        var avatarUrl = root.TryGetProperty("avatar_url", out var avatarElement) && avatarElement.ValueKind != JsonValueKind.Null
            ? avatarElement.GetString()
            : null;

        return new GitHubUserInfo
        {
            Id = id,
            Login = login,
            Name = name,
            Email = email ?? "",
            AvatarUrl = avatarUrl
        };
    }

    /// <summary>
    /// プライマリメールアドレスを取得
    /// </summary>
    private async Task<string?> FetchPrimaryEmailAsync(string accessToken, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("ServerlessKakeibo", "1.0"));

        var response = await _httpClient.SendAsync(request, cancellationToken);

        // メールアドレスへのアクセス権限がない場合は 404 が返る可能性がある
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("GitHubメールアドレスAPIへのアクセスに失敗しました。StatusCode: {StatusCode}", response.StatusCode);
            return null;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        using var json = JsonDocument.Parse(content);

        // primary かつ verified のメールアドレスを探す
        foreach (var email in json.RootElement.EnumerateArray())
        {
            var isPrimary = email.TryGetProperty("primary", out var primaryElement) && primaryElement.GetBoolean();
            var isVerified = email.TryGetProperty("verified", out var verifiedElement) && verifiedElement.GetBoolean();

            if (isPrimary && isVerified)
            {
                return email.GetProperty("email").GetString();
            }
        }

        // primary が見つからない場合、verified な最初のメールを返す
        foreach (var email in json.RootElement.EnumerateArray())
        {
            var isVerified = email.TryGetProperty("verified", out var verifiedElement) && verifiedElement.GetBoolean();

            if (isVerified)
            {
                return email.GetProperty("email").GetString();
            }
        }

        _logger.LogWarning("GitHubから確認済みメールアドレスを取得できませんでした");
        return null;
    }

    #endregion
}
