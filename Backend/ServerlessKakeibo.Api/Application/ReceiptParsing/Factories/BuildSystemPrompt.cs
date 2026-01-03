using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto.Enum;

namespace ServerlessKakeibo.Api.Application.ReceiptParsing.Factories;

public class BuildSystemPromptFactory
{
    /// <summary>
    /// システムプロンプトを構築
    /// </summary>
    public static string BuildSystemPrompt(ReceiptType? expectedType)
    {
        var basePrompt = @"あなたは領収書解析の専門家です。
提供された画像から、領収書・請求書・クレジットカード明細などの情報を正確に抽出してください。

以下のJSON形式で応答してください。情報が見つからない場合はnullを設定してください：

{
  ""receipt_type"": ""Receipt|Invoice|CreditCardSlip|Unknown"",
  ""confidence"": 0.0-1.0の数値,
  ""transaction_date"": ""yyyy-MM-dd HH:mm:ss形式（時刻が記載されていない場合は00:00:00）"",
  ""total_amount"": 金額（数値）,
  ""currency"": ""通貨コード（JPY等）"",
  ""payer"": ""支払者名"",
  ""payee"": ""受取者名（店舗名等）"",
  ""payment_method"": ""Cash|CreditCard|DebitCard|ElectronicMoney|QRCodePayment|BankTransfer|Other|Unknown"",
  ""taxes"": [
    {
      ""tax_type"": ""消費税|入湯税|宿泊税|その他"",
      ""tax_rate"": 税率（％、固定額の場合はnull）,
      ""tax_amount"": 税額,
      ""is_fixed_amount"": 固定額かどうか（true/false）,
      ""applicable_category"": ""軽減税率対象|標準税率|その他""
    }
  ],
  ""items"": [
    {
      ""name"": ""商品名"",
      ""quantity"": 数量,
      ""unit_price"": 単価,
      ""amount"": 金額
    }
  ],
  ""shop_details"": {
    ""phone_number"": ""電話番号（ハイフンあり）"",
    ""address"": ""住所"",
    ""postal_code"": ""郵便番号（ハイフンあり）"",
    ""invoice_registration_number"": ""インボイス番号（T + 13桁）"",
    ""registered_business_name"": ""インボイス登録事業者名""
  }
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
1. 日付は必ずyyyy-MM-dd HH:mm:ss形式に変換してください（時刻情報がある場合は必ず含める）
2. 金額は数値のみで、通貨記号や単位は含めないでください
3. 判定の信頼度を0.0から1.0の範囲で設定してください
4. 不明な項目はnullとしてください
5. 必ず有効なJSONを返してください
6. JSONのみを返し、説明文は含めないでください
7. 税金が複数ある場合は、taxes配列に全て含めてください
8. インボイス番号が記載されている場合は必ず抽出してください";

        if (expectedType.HasValue && expectedType != ReceiptType.Unknown)
        {
            basePrompt += $"\n\n注意：この書類は「{GetReceiptTypeName(expectedType.Value)}」である可能性が高いです。";
        }

        return basePrompt;
    }

    /// <summary>
    /// 領収書タイプの日本語名を取得
    /// </summary>
    private static string GetReceiptTypeName(ReceiptType type)
    {
        return type switch
        {
            ReceiptType.Receipt => "領収書",
            ReceiptType.Invoice => "請求書",
            ReceiptType.CreditCardSlip => "クレジットカード利用明細",
            _ => "不明な書類"
        };
    }
}
