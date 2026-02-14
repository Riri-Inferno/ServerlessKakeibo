using ServerlessKakeibo.Api.Application.ReceiptParsing.Components;
using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto;
using ServerlessKakeibo.Api.Application.ReceiptParsing.Factories;
using ServerlessKakeibo.Api.Application.ReceiptParsing.UseCase;
using ServerlessKakeibo.Api.Common.Helpers;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Domain.Receipt.Services;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Service.Interface;
using ServerlessKakeibo.Api.Service.Models;

namespace ServerlessKakeibo.Api.Application.ReceiptParsing;

/// <summary>
/// 領収書解析ユースケース実装
/// </summary>
public class ReceiptParsingInteractor : IReceiptParsingUseCase
{
    private readonly IVertexAiService _vertexAiService;
    private readonly IGenericReadRepository<UserEntity> _userRepository;
    private readonly IGoogleAiStudioService _googleAiStudioService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ReceiptParsingInteractor> _logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public ReceiptParsingInteractor(
        IVertexAiService vertexAiService,
        IGenericReadRepository<UserEntity> userRepository,
        IGoogleAiStudioService googleAiStudioService,
        IWebHostEnvironment environment,
        ILogger<ReceiptParsingInteractor> logger)
    {
        _vertexAiService = vertexAiService ?? throw new ArgumentNullException(nameof(vertexAiService));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _googleAiStudioService = googleAiStudioService ?? throw new ArgumentNullException(nameof(googleAiStudioService));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 領収書解析
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<ReceiptParseResult> ExcuteParseAsync(ReceiptParseRequest request, Guid userId)
    {
        if (request?.File == null)
        {
            throw new ArgumentNullException(nameof(request), "リクエストまたはファイルが null です");
        }

        // ユーザーの存在確認
        var user = await _userRepository.ExistsAsync(userId);
        if (!user)
        {
            throw new UnauthorizedAccessException("指定されたユーザーが存在しません");
        }

        try
        {
            _logger.LogInformation("領収書解析処理を開始します");

            // 1. 画像ファイルのバリデーション
            var (isValid, errorMessage) = await ReceiptValidationHelper.CheckFileAsReceiptImageAsync(request.File);
            if (!isValid)
            {
                _logger.LogWarning("画像検証エラー: {ErrorMessage}", errorMessage);
                throw new ArgumentException(errorMessage);
            }

            // 2. 画像をBase64に変換
            var base64Image = await ImageHelper.ConvertToBase64Async(request.File);
            var mimeType = request.File.ContentType;

            // 3. プロンプトの構築
            var systemPrompt = BuildSystemPromptFactory.BuildSystemPrompt(request.Options?.ExpectedReceiptType);
            var userPrompt = TextHelper.SanitizeCustomPrompt(request.CustomPrompt);

            // 4. VertexAI呼び出し用の画像リスト作成
            var images = new List<ImageAttachment>
            {
                new ImageAttachment
                {
                    Base64Data = base64Image,
                    MimeType = mimeType
                }
            };

            // 5. LLM API呼び出し
            #region Call External LLM Service
            string llmResponse;

            if (_environment.IsDevelopment())
            {
                // 開発環境: Google AI Studio を使用
                _logger.LogDebug("Google AI Studio APIを呼び出します");
                llmResponse = await _googleAiStudioService.GenerateContentAsync(
                    userPrompt: userPrompt,
                    systemPrompt: systemPrompt,
                    images: images
                );
            }
            else
            {
                // 本番環境: VertexAI を使用
                _logger.LogDebug("VertexAI APIを呼び出します");
                llmResponse = await _vertexAiService.GenerateContentAsync(
                    userPrompt: userPrompt,
                    systemPrompt: systemPrompt,
                    images: images
                );
            }
            #endregion

            // 6. レスポンスのJSONを検証・クリーニング
            var (isJsonValid, cleanedJson, jsonError) = JsonHelper.ExtractAndValidateLlmJson(llmResponse);
            if (!isJsonValid)
            {
                _logger.LogError("LLMレスポンスのJSON解析に失敗: {Error}", jsonError);
                throw new InvalidOperationException($"LLMレスポンスの解析に失敗しました: {jsonError}");
            }

            // 7. JSONをパースして結果オブジェクトに変換
            var result = ReceiptResponseParser.ParseLlmResponse(cleanedJson!, request.Options?.IncludeRaw ?? true);

            // 8. 結果の評価とエンリッチ
            result = ReceiptEvaluatorService.EvaluateAndEnrichResult(result);

            _logger.LogInformation("領収書解析が完了。ステータス: {Status}, 種別: {Type}, 信頼度: {Confidence}",
                result.ParseStatus, result.ReceiptType, result.Confidence);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "領収書解析中にエラーが発生しました");
            throw;
        }
    }
}
