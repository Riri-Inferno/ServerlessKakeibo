using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto;
using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto.Enum;

namespace ServerlessKakeibo.Api.Domain.Receipt.Services;

/// <summary>
/// 領収書解析結果の評価とエンリッチを行うドメインサービス
/// </summary>
public class ReceiptEvaluatorService
{
    /// <summary>
    /// 解析結果を評価し、ステータス設定や警告メッセージの追加を行う
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    public static ReceiptParseResult EvaluateAndEnrichResult(ReceiptParseResult result)
    {
        const decimal CONFIDENCE_THRESHOLD_LOW = 0.7m;
        const decimal CONFIDENCE_THRESHOLD_WARNING = 0.85m;

        // 欠落フィールドのチェック
        if (result.Normalized.TransactionDate == null)
            result.MissingFields.Add("取引日");
        if (result.Normalized.AmountTotal == null)
            result.MissingFields.Add("合計金額");
        if (string.IsNullOrWhiteSpace(result.Normalized.Payee))
            result.MissingFields.Add("受取者");

        // 明細項目の特別チェック（最優先）
        bool hasNoItems = result.Normalized.Items == null || !result.Normalized.Items.Any();
        if (hasNoItems)
        {
            result.ParseStatus = ParseStatus.Partial;
            result.Warnings.Add($"明細が取得できませんでした。信頼度: {result.Confidence:P0}");
        }

        // ステータス判定（明細がある場合のみ）
        if (!hasNoItems)
        {
            if (result.MissingFields.Any())
            {
                result.ParseStatus = ParseStatus.Partial;
                result.Warnings.Add($"以下の情報が取得できませんでした: {string.Join(", ", result.MissingFields)}");
            }
            else if (result.Confidence < CONFIDENCE_THRESHOLD_LOW)
            {
                result.ParseStatus = ParseStatus.LowConfidence;
                result.Warnings.Add($"解析の信頼度が低いです（{result.Confidence:P0}）。内容を確認してください。");
            }
            else if (result.Confidence < CONFIDENCE_THRESHOLD_WARNING)
            {
                result.ParseStatus = ParseStatus.Complete;
                result.Warnings.Add($"信頼度: {result.Confidence:P0}");
            }
            else
            {
                result.ParseStatus = ParseStatus.Complete;
            }
        }
        else if (result.MissingFields.Any())
        {
            result.Warnings.Add($"さらに以下の情報も取得できませんでした: {string.Join(", ", result.MissingFields)}");
        }

        // インボイス番号の検証
        if (result.Normalized.ShopDetails?.InvoiceRegistrationNumber != null)
        {
            if (!ValidateInvoiceNumber(result.Normalized.ShopDetails.InvoiceRegistrationNumber))
            {
                result.Warnings.Add("インボイス番号の形式が正しくない可能性があります");
            }
        }

        return result;
    }

    /// <summary>
    /// インボイス番号の形式を検証
    /// </summary>
    /// <param name="invoiceNumber"></param>
    /// <returns></returns>
    private static bool ValidateInvoiceNumber(string invoiceNumber)
    {
        // ハイフンを除去してから検証
        var cleanedNumber = invoiceNumber.Replace("-", "");

        // T + 13桁の数字
        return System.Text.RegularExpressions.Regex.IsMatch(
            cleanedNumber,
            @"^T\d{13}$"
        );
    }
}
