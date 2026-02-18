namespace ServerlessKakeibo.Api.Domain.Transaction.Models;

/// <summary>
/// 取引項目ドメインモデル
/// </summary>
public class TransactionItem
{
    public string? Name { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? Amount { get; set; }

    /// <summary>
    /// ドメインルール：金額の整合性チェック
    /// </summary>
    public bool IsAmountValid()
    {
        if (Quantity == null || UnitPrice == null || Amount == null)
            return true; // データ不足の場合は検証スキップ

        var calculatedAmount = Quantity.Value * UnitPrice.Value;
        return Math.Abs(calculatedAmount - Amount.Value) < 0.01m;
    }
}
