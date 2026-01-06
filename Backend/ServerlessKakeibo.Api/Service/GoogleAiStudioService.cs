using Microsoft.Extensions.Options;
using ServerlessKakeibo.Api.Service.Interface;
using ServerlessKakeibo.Api.Common.Settings;
using ServerlessKakeibo.Api.Common.Exceptions;
using ServerlessKakeibo.Api.Service.Models;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;
using System.Net;

namespace ServerlessKakeibo.Api.Service;

/// <summary>
/// Google AI Studio呼び出しサービス
/// </summary>
public class GoogleAiStudioService : IGoogleAiStudioService
{
    private readonly GoogleAiStudioSettings _settings;
    private readonly ILogger<GoogleAiStudioService> _logger;
    private readonly GenerativeModel _model;

    public GoogleAiStudioService(
        IOptions<GoogleAiStudioSettings> settings,
        ILogger<GoogleAiStudioService> logger)
    {
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var googleAI = new GoogleAI(_settings.ApiKey);

        // モデルを生成
        _model = googleAI.GenerativeModel(_settings.DefaultModelId);
    }

    /// <summary>
    /// LLMによるテキスト生成
    /// </summary>
    /// <param name="userPrompt"></param>
    /// <param name="systemPrompt"></param>
    /// <param name="images"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="CustomException"></exception>
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
            _logger.LogDebug("Mscc.GenerativeAI SDKを使用してAPI呼び出しを開始します");

            // 1. 生成設定の構築
            var config = new GenerationConfig
            {
                Temperature = (float?)_settings.Temperature,
                TopP = (float?)_settings.TopP,
                TopK = _settings.TopK,
                CandidateCount = _settings.CandidateCount,
                MaxOutputTokens = _settings.MaxOutputTokens
            };

            // 2. リクエストオブジェクトの構築
            var request = new GenerateContentRequest
            {
                Model = _settings.DefaultModelId,
                GenerationConfig = config,
                Contents = new List<Content>()
            };

            // 3. ユーザーコンテンツの構築
            var userParts = new List<IPart>();

            // システムプロンプトとユーザープロンプトを結合（VertexAIと同じロジック）
            var combinedPrompt = string.IsNullOrWhiteSpace(systemPrompt)
                ? userPrompt
                : $"{systemPrompt}\n\n{userPrompt}";

            _logger.LogDebug("プロンプトを構築しました");

            // テキストパーツの追加
            userParts.Add(new TextData { Text = combinedPrompt });

            // 画像パーツの追加
            if (images != null && images.Count > 0)
            {
                foreach (var image in images)
                {
                    if (string.IsNullOrWhiteSpace(image.Base64Data))
                    {
                        _logger.LogWarning("空の画像データがスキップされました");
                        continue;
                    }

                    userParts.Add(new InlineData
                    {
                        MimeType = image.MimeType,
                        Data = image.Base64Data
                    });
                }
            }

            // userロールでコンテンツを作成（VertexAIと同じ）
            var userContent = new Content(userParts, Role.User);
            request.Contents.Add(userContent);

            // 4. API呼び出しの実行
            var response = await _model.GenerateContent(request);

            _logger.LogDebug("Google AI Studio API呼び出しが正常に完了しました");

            return response.Text ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google AI Studio APIの呼び出し中にエラーが発生しました");
            throw new CustomException(
                new ExceptionType(HttpStatusCode.InternalServerError, "AI生成エラー"),
                ex);
        }
    }
}
