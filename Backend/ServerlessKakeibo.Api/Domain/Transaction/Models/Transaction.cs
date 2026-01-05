using ServerlessKakeibo.Api.Domain.Receipt.Models;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Domain.Transaction.Models;

/// <summary>
/// 取引ドメインモデル
/// </summary>
public class Transaction
{
    public Guid Id { get; set; }
    public DateTimeOffset? TransactionDate { get; set; }
    public decimal? AmountTotal { get; set; }
    public string Currency { get; set; } = "JPY";
    public string? Payer { get; set; }
    public string? Payee { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }

    public List<TransactionItem> Items { get; set; } = new();
    public List<TaxDetail> Taxes { get; set; } = new();
    public ShopDetails? ShopDetails { get; set; }

    /// <summary>
    /// ドメインルール：取引の整合性チェック
    /// </summary>
    public bool IsValid()
    {
        if (AmountTotal == null || AmountTotal <= 0)
            return false;

        // 項目の合計と取引金額が一致するか
        var itemsTotal = Items.Sum(i => i.Amount ?? 0);
        if (itemsTotal > 0 && Math.Abs(itemsTotal - AmountTotal.Value) > 0.01m)
            return false;

        return true;
    }

    /// <summary>
    /// ドメインルール：税額の整合性チェック
    /// </summary>
    public bool AreTaxesValid()
    {
        var taxTotal = Taxes.Sum(t => t.TaxAmount ?? 0);
        // TODO: 税額の検証ロジック
        return true;
    }
}
