using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto;
using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto.Enum;
using ServerlessKakeibo.Api.Application.ReceiptParsing.UseCase;
using ServerlessKakeibo.Api.Common.Helpers;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Service.Interface;
using ServerlessKakeibo.Api.Service.Models;
using System.Text.Json;

namespace ServerlessKakeibo.Api.Application.ReceiptParsing;

/// <summary>
/// 領収書解析ユースケース実装
/// </summary>
public class ReceiptParsingInteractor : IReceiptParsingUseCase
{
    private readonly IVertexAiService _vertexAiService;
    private readonly ILogger<ReceiptParsingInteractor> _logger;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public ReceiptParsingInteractor(
        IVertexAiService vertexAiService,
        ILogger<ReceiptParsingInteractor> logger)
    {
        _vertexAiService = vertexAiService ?? throw new ArgumentNullException(nameof(vertexAiService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 領収書解析
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<ReceiptParseResult> ExcuteParseAsync(ReceiptParseRequest request)
    {
        if (request?.File == null)
        {
            throw new ArgumentNullException(nameof(request), "リクエストまたはファイルが null です");
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
            var systemPrompt = BuildSystemPrompt(request.Options?.ExpectedReceiptType);
            var userPrompt = request.CustomPrompt ?? "この画像を解析してください。";

            // 4. VertexAI呼び出し用の画像リスト作成
            var images = new List<ImageAttachment>
            {
                new ImageAttachment
                {
                    Base64Data = base64Image,
                    MimeType = mimeType
                }
            };

            // 5. VertexAI API呼び出し
            _logger.LogDebug("VertexAI APIを呼び出します");
            var llmResponse = await _vertexAiService.GenerateContentAsync(
                userPrompt: userPrompt,
                systemPrompt: systemPrompt,
                images: images
            );

            // 6. レスポンスのJSONを検証・クリーニング
            var (isJsonValid, cleanedJson, jsonError) = JsonHelper.ExtractAndValidateLlmJson(llmResponse);
            if (!isJsonValid)
            {
                _logger.LogError("LLMレスポンスのJSON解析に失敗: {Error}", jsonError);
                throw new InvalidOperationException($"LLMレスポンスの解析に失敗しました: {jsonError}");
            }

            // 7. JSONをパースして結果オブジェクトに変換
            var result = ParseLlmResponse(cleanedJson!, request.Options?.IncludeRaw ?? true);

            _logger.LogInformation("領収書解析が正常に完了しました。種別: {ReceiptType}, 信頼度: {Confidence}",
                result.ReceiptType, result.Confidence);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "領収書解析中にエラーが発生しました");
            throw;
        }
    }

    /// <summary>
    /// システムプロンプトを構築
    /// </summary>
    private string BuildSystemPrompt(ReceiptType? expectedType)
    {
        var basePrompt = @"あなたは領収書解析の専門家です。
提供された画像から、領収書・請求書・クレジットカード明細などの情報を正確に抽出してください。

以下のJSON形式で応答してください。情報が見つからない場合はnullを設定してください：

{
  ""receipt_type"": ""Receipt|Invoice|CreditCardSlip|Unknown"",
  ""confidence"": 0.0-1.0の数値,
  ""transaction_date"": ""yyyy-MM-dd形式の日付"",
  ""total_amount"": 金額（数値）,
  ""currency"": ""通貨コード（JPY等）"",
  ""payer"": ""支払者名"",
  ""payee"": ""受取者名（店舗名等）"",
  ""payment_method"": ""Cash|CreditCard|DebitCard|ElectronicMoney|QRCodePayment|BankTransfer|Other|Unknown"",
  ""tax_info"": {
    ""tax_rate"": 税率（パーセント）,
    ""tax_amount"": 税額
  },
  ""items"": [
    {
      ""name"": ""商品名"",
      ""quantity"": 数量,
      ""unit_price"": 単価,
      ""amount"": 金額
    }
  ]
}

支払方法の判定基準：
- Cash: 現金
- CreditCard: クレジットカード
- DebitCard: デビットカード
- ElectronicMoney: 交通系ICカード（Suica、PASMO等）、楽天Edy、WAON、nanaco等
- QRCodePayment: PayPay、楽天Pay、LINE Pay、メルペイ、d払い等
- BankTransfer: 銀行振込
- Other: その他の支払方法
- Unknown: 不明または判定不可

重要な指示：
1. 日付は必ずyyyy-MM-dd形式に変換してください
2. 金額は数値のみで、通貨記号や単位は含めないでください
3. 判定の信頼度を0.0から1.0の範囲で設定してください
4. 不明な項目はnullとしてください
5. 必ず有効なJSONを返してください
6. JSONのみを返し、説明文は含めないでください";

        if (expectedType.HasValue && expectedType != ReceiptType.Unknown)
        {
            basePrompt += $"\n\n注意：この書類は「{GetReceiptTypeName(expectedType.Value)}」である可能性が高いです。";
        }

        return basePrompt;
    }

    /// <summary>
    /// 領収書タイプの日本語名を取得
    /// </summary>
    private string GetReceiptTypeName(ReceiptType type)
    {
        return type switch
        {
            ReceiptType.Receipt => "領収書",
            ReceiptType.Invoice => "請求書",
            ReceiptType.CreditCardSlip => "クレジットカード利用明細",
            _ => "不明な書類"
        };
    }

    /// <summary>
    /// LLMレスポンスをパースして結果オブジェクトに変換
    /// </summary>
    private ReceiptParseResult ParseLlmResponse(string cleanedJson, bool includeRaw)
    {
        try
        {
            using var jsonDoc = JsonDocument.Parse(cleanedJson);
            var root = jsonDoc.RootElement;

            var result = new ReceiptParseResult
            {
                // 領収書タイプをパース
                ReceiptType = ParseReceiptType(root.GetProperty("receipt_type").GetString()),

                // 信頼度をパース
                Confidence = root.TryGetProperty("confidence", out var confProp)
                    ? confProp.GetDecimal()
                    : 0.5m,

                // 正規化された取引情報
                Normalized = new NormalizedTransaction
                {
                    TransactionDate = ParseDateFromJson(root, "transaction_date"),
                    AmountTotal = ParseDecimalFromJson(root, "total_amount"),
                    Currency = root.TryGetProperty("currency", out var currProp)
                        ? currProp.GetString() ?? "JPY"
                        : "JPY",
                    Payer = GetStringOrNull(root, "payer"),
                    Payee = GetStringOrNull(root, "payee"),
                    PaymentMethod = ParsePaymentMethod(root, "payment_method"),
                    Tax = ParseTaxInfo(root),
                    Items = ParseItems(root)
                }
            };

            // 生データを含める場合
            if (includeRaw)
            {
                result.Raw = jsonDoc.RootElement.Clone();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLMレスポンスのパース中にエラーが発生しました: {Json}", cleanedJson);

            // パースエラー時はデフォルト値を返す
            return new ReceiptParseResult
            {
                ReceiptType = ReceiptType.Unknown,
                Confidence = 0,
                Normalized = new NormalizedTransaction
                {
                    Currency = "JPY",
                    Items = new List<NormalizedItem>()
                }
            };
        }
    }

    /// <summary>
    /// 領収書タイプをパース
    /// </summary>
    private ReceiptType ParseReceiptType(string? typeString)
    {
        if (string.IsNullOrWhiteSpace(typeString))
            return ReceiptType.Unknown;

        return typeString.ToUpperInvariant() switch
        {
            "RECEIPT" => ReceiptType.Receipt,
            "INVOICE" => ReceiptType.Invoice,
            "CREDITCARDSLIP" => ReceiptType.CreditCardSlip,
            _ => ReceiptType.Unknown
        };
    }

    /// <summary>
    /// 支払方法をパース
    /// </summary>
    private PaymentMethod? ParsePaymentMethod(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var prop))
            return null;

        var methodString = prop.GetString();
        if (string.IsNullOrWhiteSpace(methodString))
            return null;

        return methodString.ToUpperInvariant() switch
        {
            "CASH" => PaymentMethod.Cash,
            "CREDITCARD" => PaymentMethod.CreditCard,
            "DEBITCARD" => PaymentMethod.DebitCard,
            "ELECTRONICMONEY" => PaymentMethod.ElectronicMoney,
            "QRCODEPAYMENT" => PaymentMethod.QRCodePayment,
            "BANKTRANSFER" => PaymentMethod.BankTransfer,
            "OTHER" => PaymentMethod.Other,
            _ => PaymentMethod.Unknown
        };
    }

    /// <summary>
    /// 日付をパース
    /// </summary>
    private DateTimeOffset? ParseDateFromJson(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var prop))
            return null;

        var dateString = prop.GetString();
        return ReceiptValidationHelper.ParseDateString(dateString);
    }

    /// <summary>
    /// 数値をパース
    /// </summary>
    private decimal? ParseDecimalFromJson(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var prop))
            return null;

        if (prop.ValueKind == JsonValueKind.Number)
            return prop.GetDecimal();

        if (prop.ValueKind == JsonValueKind.String)
            return ReceiptValidationHelper.ParseAmountString(prop.GetString());

        return null;
    }

    /// <summary>
    /// 文字列を取得（nullの場合はnull）
    /// </summary>
    private string? GetStringOrNull(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var prop))
            return null;

        if (prop.ValueKind == JsonValueKind.Null)
            return null;

        return prop.GetString();
    }

    /// <summary>
    /// 税情報をパース
    /// </summary>
    private TaxInfo? ParseTaxInfo(JsonElement root)
    {
        if (!root.TryGetProperty("tax_info", out var taxProp) ||
            taxProp.ValueKind == JsonValueKind.Null)
            return null;

        return new TaxInfo
        {
            TaxRate = taxProp.TryGetProperty("tax_rate", out var rateProp)
                ? (int?)rateProp.GetInt32()
                : null,
            TaxAmount = ParseDecimalFromJson(taxProp, "tax_amount")
        };
    }

    /// <summary>
    /// 商品明細をパース
    /// </summary>
    private List<NormalizedItem> ParseItems(JsonElement root)
    {
        var items = new List<NormalizedItem>();

        if (!root.TryGetProperty("items", out var itemsProp) ||
            itemsProp.ValueKind != JsonValueKind.Array)
            return items;

        foreach (var itemElement in itemsProp.EnumerateArray())
        {
            var item = new NormalizedItem
            {
                Name = GetStringOrNull(itemElement, "name"),
                Quantity = ParseDecimalFromJson(itemElement, "quantity"),
                UnitPrice = ParseDecimalFromJson(itemElement, "unit_price"),
                Amount = ParseDecimalFromJson(itemElement, "amount")
            };

            items.Add(item);
        }

        return items;
    }
}
