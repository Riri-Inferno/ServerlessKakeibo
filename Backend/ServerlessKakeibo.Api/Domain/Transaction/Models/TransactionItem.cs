using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Domain.Transaction.Models;

/// <summary>
/// 取引項目ドメインモデル
/// </summary>
public class TransactionItem
{
    /// <summary>
    /// 項目種別（商品/値引き）
    /// </summary>
    public TransactionItemType ItemType { get; set; } = TransactionItemType.Product;

    public string? Name { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? Amount { get; set; }

    /// <summary>
    /// ドメインルール：金額の整合性チェック
    /// </summary>
    /// <remarks>
    /// 値引き(Discount)の場合は Amount が 0 以下、商品(Product)の場合は 0 以上であることを要求する。
    /// 数量・単価は値引きでは省略されることがあるため null 許容。
    /// </remarks>
    public bool IsAmountValid()
    {
        if (Amount.HasValue)
        {
            if (ItemType == TransactionItemType.Discount && Amount.Value > 0)
                return false;

            if (ItemType == TransactionItemType.Product && Amount.Value < 0)
                return false;
        }

        if (Quantity == null || UnitPrice == null || Amount == null)
            return true; // データ不足の場合は計算検証スキップ

        var calculatedAmount = Quantity.Value * UnitPrice.Value;
        return Math.Abs(calculatedAmount - Amount.Value) < 0.01m;
    }
}
