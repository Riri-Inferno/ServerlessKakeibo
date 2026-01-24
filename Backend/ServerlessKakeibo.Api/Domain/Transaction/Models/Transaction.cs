using ServerlessKakeibo.Api.Domain.Receipt.Models;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Domain.Transaction.Models;

/// <summary>
/// 取引ドメインモデル
/// </summary>
public class Transaction
{
    public Guid Id { get; set; }
    public TransactionType Type { get; set; } = TransactionType.Expense;
    public DateTimeOffset? TransactionDate { get; set; }
    public decimal? AmountTotal { get; set; }
    public string Currency { get; set; } = "JPY";
    public string? Payer { get; set; }
    public string? Payee { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public TaxInclusionType TaxInclusionType { get; set; } = TaxInclusionType.Unknown;
    public List<TransactionItem> Items { get; set; } = new();
    public List<TaxDetail> Taxes { get; set; } = new();
    public ShopDetails? ShopDetails { get; set; }

    /// <summary>
    /// ドメインルール: 取引の整合性チェック
    /// </summary>
    public bool IsValid()
    {
        // 金額が0以下は不正
        if (AmountTotal == null || AmountTotal <= 0)
            return false;

        // 支出の場合のみ、項目合計の整合性をチェック
        if (Type == TransactionType.Expense && Items.Any())
        {
            var itemsTotal = Items.Sum(i => i.Amount ?? 0);
            var taxTotal = Taxes.Sum(t => t.TaxAmount ?? 0);

            decimal calculatedTotal;

            // 税の扱いに応じて期待される合計額を計算
            if (TaxInclusionType == TaxInclusionType.Inclusive ||
                TaxInclusionType == TaxInclusionType.NoTax)
            {
                // 内税または非課税：itemsTotal がそのまま合計
                calculatedTotal = itemsTotal;
            }
            else // Exclusive または Unknown
            {
                // 外税：itemsTotal + 税額
                calculatedTotal = itemsTotal + taxTotal;
            }

            // 1円以内の誤差を許容(丸め誤差対策)
            if (Math.Abs(calculatedTotal - AmountTotal.Value) > 1.0m)
                return false;
        }

        // 収入の場合は AmountTotal のみで OK（Items は任意）

        return true;
    }

    /// <summary>
    /// ドメインルール: 税額の整合性チェック
    /// </summary>
    public bool AreTaxesValid()
    {
        if (!Taxes.Any())
            return true;

        var itemsTotal = Items.Sum(i => i.Amount ?? 0);
        if (itemsTotal <= 0)
            return true;

        // 税率が設定されている税情報のみ検証
        foreach (var tax in Taxes)
        {
            // 税率がnullの場合は固定額として扱うのでスキップ
            if (!tax.TaxRate.HasValue)
                continue;

            // 税額がnullの場合はスキップ
            if (!tax.TaxAmount.HasValue)
                continue;

            var taxRate = tax.TaxRate.Value;
            var taxAmount = tax.TaxAmount.Value;

            // 課税対象額が指定されている場合はそれを使用、なければ項目合計を使用
            var taxableAmount = tax.TaxableAmount ?? itemsTotal;

            // 期待される税額を計算
            var expectedTax = taxableAmount * (taxRate / 100m);

            // 税額が期待値から大きく外れていないかチェック
            var diff = Math.Abs(taxAmount - expectedTax);
            var tolerance = Math.Max(expectedTax * 0.15m, 2.0m); // 15%または2円の誤差許容

            if (diff > tolerance)
                return false;
        }

        return true;
    }
}
