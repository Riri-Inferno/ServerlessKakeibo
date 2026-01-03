using System.Text.Json;
using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto;
using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto.Enum;
using ServerlessKakeibo.Api.Common.Helpers;
using ServerlessKakeibo.Api.Domain.Receipt.Models;

namespace ServerlessKakeibo.Api.Application.ReceiptParsing.Components;

/// <summary>
/// LLMレスポンスのパースコンポーネント
/// </summary>
public class ReceiptResponseParser
{
    /// <summary>
    /// LLMレスポンスをパースして結果オブジェクトに変換
    /// </summary>
    public static ReceiptParseResult ParseLlmResponse(string cleanedJson, bool includeRaw)
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
                    AmountTotal = JsonHelper.ParseDecimalFromJson(root, "total_amount"),
                    Currency = root.TryGetProperty("currency", out var currProp)
                        ? currProp.GetString() ?? "JPY"
                        : "JPY",
                    Payer = JsonHelper.GetStringOrNull(root, "payer"),
                    Payee = JsonHelper.GetStringOrNull(root, "payee"),
                    PaymentMethod = ParsePaymentMethod(root, "payment_method"),
                    Taxes = ParseTaxes(root),
                    Items = ParseItems(root),
                    ShopDetails = ParseShopDetails(root)
                },

                // 初期ステータス
                ParseStatus = ParseStatus.Complete,
                Warnings = new List<string>(),
                MissingFields = new List<string>()
            };

            // 生データを含める場合
            if (includeRaw)
            {
                result.Raw = jsonDoc.RootElement.Clone();
            }

            return result;
        }
        catch (Exception)
        {
            // パースエラー時はデフォルト値を返す
            return new ReceiptParseResult
            {
                ReceiptType = ReceiptType.Unknown,
                Confidence = 0,
                Normalized = new NormalizedTransaction
                {
                    Currency = "JPY",
                    Items = new List<NormalizedItem>(),
                    Taxes = new List<TaxDetail>()
                },
                ParseStatus = ParseStatus.Failed,
                Warnings = new List<string> { "解析に失敗しました" },
                MissingFields = new List<string>()
            };
        }
    }

    /// <summary>
    /// 領収書タイプをパース
    /// </summary>
    private static ReceiptType ParseReceiptType(string? typeString)
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
    /// 日付をパース
    /// </summary>
    private static DateTimeOffset? ParseDateFromJson(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var prop))
            return null;

        var dateString = prop.GetString();
        return ReceiptValidationHelper.ParseDateString(dateString);
    }

    /// <summary>
    /// 支払方法をパース
    /// </summary>
    private static PaymentMethod? ParsePaymentMethod(JsonElement root, string propertyName)
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
    /// 税情報リストをパース
    /// </summary>
    private static List<TaxDetail> ParseTaxes(JsonElement root)
    {
        var taxes = new List<TaxDetail>();

        // taxes配列から取得
        if (root.TryGetProperty("taxes", out var taxesProp) &&
            taxesProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var taxElement in taxesProp.EnumerateArray())
            {
                var tax = new TaxDetail
                {
                    TaxType = JsonHelper.GetStringOrNull(taxElement, "tax_type") ?? "消費税",
                    TaxRate = JsonHelper.ParseTaxRateFromJson(taxElement, "tax_rate"),
                    TaxAmount = JsonHelper.ParseDecimalFromJson(taxElement, "tax_amount"),
                    IsFixedAmount = taxElement.TryGetProperty("is_fixed_amount", out var fixedProp)
                        ? fixedProp.GetBoolean()
                        : false,
                    ApplicableCategory = JsonHelper.GetStringOrNull(taxElement, "applicable_category")
                };
                taxes.Add(tax);
            }
        }

        return taxes;
    }

    /// <summary>
    /// 店舗詳細情報をパース
    /// </summary>
    private static ShopDetails? ParseShopDetails(JsonElement root)
    {
        if (!root.TryGetProperty("shop_details", out var shopProp) ||
            shopProp.ValueKind == JsonValueKind.Null)
            return null;

        return new ShopDetails
        {
            PhoneNumber = JsonHelper.GetStringOrNull(shopProp, "phone_number"),
            Address = JsonHelper.GetStringOrNull(shopProp, "address"),
            PostalCode = JsonHelper.GetStringOrNull(shopProp, "postal_code"),
            InvoiceRegistrationNumber = JsonHelper.GetStringOrNull(shopProp, "invoice_registration_number"),
            RegisteredBusinessName = JsonHelper.GetStringOrNull(shopProp, "registered_business_name")
        };
    }

    /// <summary>
    /// 商品明細をパース
    /// </summary>
    private static List<NormalizedItem> ParseItems(JsonElement root)
    {
        var items = new List<NormalizedItem>();

        if (!root.TryGetProperty("items", out var itemsProp) ||
            itemsProp.ValueKind != JsonValueKind.Array)
            return items;

        foreach (var itemElement in itemsProp.EnumerateArray())
        {
            var item = new NormalizedItem
            {
                Name = JsonHelper.GetStringOrNull(itemElement, "name"),
                Quantity = JsonHelper.ParseDecimalFromJson(itemElement, "quantity") ?? 1.0m,
                UnitPrice = JsonHelper.ParseDecimalFromJson(itemElement, "unit_price"),
                Amount = JsonHelper.ParseDecimalFromJson(itemElement, "amount")
            };

            items.Add(item);
        }

        return items;
    }
}
