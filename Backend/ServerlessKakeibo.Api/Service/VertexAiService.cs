using Microsoft.Extensions.Options;
using ServerlessKakeibo.Api.Service.Interface;
using ServerlessKakeibo.Common.Settings;
using ServerlessKakeibo.Common.Exceptions;
using ServerlessKakeibo.Api.Service.Models;
using System.Net.Http.Headers;
using System.Net;
using System.Text.Json;
using System.Text;

namespace ServerlessKakeibo.Api.Service;

/// <summary>
/// Vertex AI サービス
/// </summary>
public class VertexAiService : IVertexAiService
{
    private readonly VertexAiSettings _settings;
    private readonly IGcpAuthService _authService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<VertexAiService> _logger;


    /// <summary>
    /// コンストラクタ
    /// </summary>
    public VertexAiService(
        IOptions<VertexAiSettings> settings,
        IGcpAuthService authService,
        IHttpClientFactory httpClientFactory,
        ILogger<VertexAiService> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// テキスト生成を実行
    /// </summary>
    /// <param name="userPrompt">ユーザープロンプト</param>
    /// <param name="systemPrompt">システムプロンプト（オプション）</param>
    /// <param name="images">画像のリスト（オプション）</param>
    /// <returns>生成されたテキスト</returns>
    public async Task<string> GenerateContentAsync(
        string userPrompt,
        string? systemPrompt = null,
        List<ImageAttachment>? images = null)
    {
        if (string.IsNullOrWhiteSpace(userPrompt))
        {
            throw new ArgumentException("ユーザープロンプトが必要です", nameof(userPrompt));
        }

        try
        {
            _logger.LogDebug("Vertex AI API呼び出しを開始します");

            // アクセストークン取得
            var accessToken = await _authService.GetAccessTokenAsync(
                "https://www.googleapis.com/auth/cloud-platform");

            // APIエンドポイント構築
            var apiUrl = $"https://{_settings.Location}-aiplatform.googleapis.com/v1/projects/{_settings.ProjectId}/locations/{_settings.Location}/publishers/google/models/{_settings.DefaultModelId}:generateContent";

            // リクエストボディ構築
            var requestBody = BuildRequestBody(userPrompt, systemPrompt, images);

            // HTTP呼び出し実行
            var response = await ExecuteHttpRequestAsync(apiUrl, requestBody, accessToken);

            // レスポンス解析
            var result = ParseResponse(response);

            _logger.LogDebug("Vertex AI API呼び出しが正常に完了しました");

            return result;
        }
        catch (CustomException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vertex AI APIの呼び出し中にエラーが発生しました");
            throw new CustomException(
                new ExceptionType(
                    HttpStatusCode.InternalServerError,
                    ErrorMessages.VertexAi.ApiCallFailed),
                ex);
        }
    }

    /// <summary>
    /// リクエストボディを構築
    /// </summary>
    private object BuildRequestBody(
        string userPrompt,
        string? systemPrompt,
        List<ImageAttachment>? images)
    {
        var parts = new List<object>();

        // システムプロンプトとユーザープロンプトを結合
        var combinedPrompt = string.IsNullOrWhiteSpace(systemPrompt)
            ? userPrompt
            : $"{systemPrompt}\n\n{userPrompt}";

        parts.Add(new { text = combinedPrompt });

        // 画像を追加（存在する場合）
        if (images != null && images.Count > 0)
        {
            foreach (var image in images)
            {
                if (string.IsNullOrWhiteSpace(image.Base64Data))
                {
                    _logger.LogWarning("空の画像データがスキップされました");
                    continue;
                }

                parts.Add(new
                {
                    inline_data = new
                    {
                        mime_type = image.MimeType,
                        data = image.Base64Data
                    }
                });
            }
        }

        // リクエストボディ（設定値を直接使用）
        var baseRequest = new
        {
            contents = new[]
            {
               new
               {
                   role = "user",
                   parts = parts.ToArray()
               }
           },
            generationConfig = new
            {
                temperature = _settings.Temperature,
                topP = _settings.TopP,
                topK = _settings.TopK,
                candidateCount = _settings.CandidateCount,
                maxOutputTokens = _settings.MaxOutputTokens
            }
        };

        // セーフティ設定を明示的に制御
        if (_settings.EnableSafetyFiltering)
        {
            // セーフティ有効（通常のフィルタリング）
            return new
            {
                baseRequest.contents,
                baseRequest.generationConfig,
                safetySettings = new[]
                {
                   new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                   new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                   new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                   new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                   new { category = "HARM_CATEGORY_CIVIC_INTEGRITY", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
               }
            };
        }
        else
        {
            // セーフティ無効（すべてBLOCK_NONE）
            return new
            {
                baseRequest.contents,
                baseRequest.generationConfig,
                safetySettings = new[]
                {
                   new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                   new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                   new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                   new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" },
                   new { category = "HARM_CATEGORY_CIVIC_INTEGRITY", threshold = "BLOCK_NONE" }
               }
            };
        }
    }

    /// <summary>
    /// HTTP リクエストを実行
    /// </summary>
    private async Task<string> ExecuteHttpRequestAsync(
        string apiUrl,
        object requestBody,
        string accessToken)
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        var jsonContent = JsonSerializer.Serialize(requestBody);
        var retryCount = 0;

        while (retryCount <= _settings.MaxRetries)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl)
                {
                    Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();

                    // レート制限の場合はリトライ
                    if (response.StatusCode == HttpStatusCode.TooManyRequests && retryCount < _settings.MaxRetries)
                    {
                        retryCount++;
                        _logger.LogWarning("{Message}。リトライ {RetryCount}/{MaxRetries}",
                             ErrorMessages.VertexAi.RateLimitExceeded, retryCount, _settings.MaxRetries);
                        await Task.Delay(_settings.RetryDelayMilliseconds * retryCount);
                        continue;
                    }

                    throw new CustomException(
                        new ExceptionType(
                            response.StatusCode,
                            $"Vertex AI API呼び出しエラー: {errorContent}"));
                }

                return await response.Content.ReadAsStringAsync();
            }
            catch (TaskCanceledException)
            {
                throw new CustomException(
                    new ExceptionType(
                        HttpStatusCode.RequestTimeout,
                        ErrorMessages.VertexAi.RequestTimeout));
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                retryCount++;
                if (retryCount > _settings.MaxRetries)
                {
                    throw new CustomException(
                        new ExceptionType(
                            HttpStatusCode.InternalServerError,
                            ErrorMessages.VertexAi.ApiCallFailed),
                        ex);
                }

                _logger.LogWarning(ex, "API呼び出しエラー。リトライ {RetryCount}/{MaxRetries}",
                    retryCount, _settings.MaxRetries);
                await Task.Delay(_settings.RetryDelayMilliseconds * retryCount);
            }
        }

        throw new CustomException(
         new ExceptionType(
             HttpStatusCode.InternalServerError,
             ErrorMessages.VertexAi.MaxRetriesReached));
    }

    /// <summary>
    /// APIレスポンスを解析
    /// </summary>
    private string ParseResponse(string jsonResponse)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(jsonResponse);

            // candidatesの確認
            if (!jsonDoc.RootElement.TryGetProperty("candidates", out var candidates) ||
                candidates.GetArrayLength() == 0)
            {
                throw new CustomException(
                 new ExceptionType(
                     HttpStatusCode.InternalServerError,
                     ErrorMessages.VertexAi.EmptyResponse));
            }

            // 生成テキストの取得
            var generatedText = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            return generatedText;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "レスポンスの解析に失敗しました: {Response}", jsonResponse);
            throw new CustomException(
             new ExceptionType(
                 HttpStatusCode.InternalServerError,
                 ErrorMessages.VertexAi.ResponseParseFailed),
             ex);
        }
    }
}
