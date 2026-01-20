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

        // 金額整合性チェック
        result.Normalized.AmountValidation = BuildAmountValidation(result.Normalized);

        if (result.Normalized.AmountValidation != null &&
            !IsAmountConsistent(result.Normalized.AmountValidation))
        {
            result.Warnings.Add(
                "明細・税額と合計金額の整合性が取れていません（内税・外税いずれでも一致しません）"
            );
        }

        return result;
    }

    /// <summary>
    /// AmountValidation オブジェクトを構築
    /// </summary>
    private static AmountValidationResult? BuildAmountValidation(NormalizedTransaction tx)
    {
        if (tx.AmountTotal == null)
            return null;

        var itemTotal = tx.Items?
            .Where(i => i.Amount != null)
            .Sum(i => i.Amount!.Value) ?? 0m;

        var taxTotal = tx.Taxes?
            .Where(t => t.TaxAmount != null)
            .Sum(t => t.TaxAmount!.Value) ?? 0m;

        var amountTotal = tx.AmountTotal.Value;

        const decimal TOLERANCE = 0.01m; // 1円未満の誤差を許容

        // 外税：小計 + 税 = 合計
        bool matchesExternalTax = Math.Abs((itemTotal + taxTotal) - amountTotal) < TOLERANCE;

        // 内税：小計 = 合計（税は内包）
        bool matchesInternalTax = Math.Abs(itemTotal - amountTotal) < TOLERANCE;

        return new AmountValidationResult
        {
            ItemsTotal = itemTotal,
            TaxTotal = taxTotal,
            MatchesAsExclusiveTax = matchesExternalTax,  // 外税として一致
            MatchesAsInclusiveTax = matchesInternalTax   // 内税として一致
        };
    }

    /// <summary>
    /// 金額が整合しているかチェック
    /// </summary>
    private static bool IsAmountConsistent(AmountValidationResult validation)
    {
        return validation.MatchesAsExclusiveTax == true ||
               validation.MatchesAsInclusiveTax == true;
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

    /// <summary>
    /// 金額の整合性チェック
    /// </summary>
    /// <param name="MatchesInternalTax"></param>
    /// <param name="MatchesExternalTax"></param>
    private readonly record struct AmountConsistencyResult(
        bool MatchesInternalTax,
        bool MatchesExternalTax
    )
    {
        public bool IsConsistent => MatchesInternalTax || MatchesExternalTax;

        public static AmountConsistencyResult NotEvaluatable =>
            new(false, false);
    }
}
