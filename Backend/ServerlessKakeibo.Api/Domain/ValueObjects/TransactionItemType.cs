using System.ComponentModel;

namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// 取引項目種別（商品/値引き）
/// </summary>
public enum TransactionItemType
{
    /// <summary>
    /// 商品（通常の取引項目、Amount は 0 以上）
    /// </summary>
    [Description("商品")]
    Product = 0,

    /// <summary>
    /// 値引き（セット値引・クーポン等、Amount は 0 以下のマイナス値）
    /// </summary>
    [Description("値引き")]
    Discount = 1
}

/// <summary>
/// TransactionItemType 拡張メソッド
/// </summary>
public static class TransactionItemTypeExtensions
{
    /// <summary>
    /// 日本語名を取得
    /// </summary>
    public static string ToJapanese(this TransactionItemType type)
    {
        var enumType = typeof(TransactionItemType);
        var memberInfo = enumType.GetMember(type.ToString());

        if (memberInfo.Length > 0)
        {
            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return ((DescriptionAttribute)attributes[0]).Description;
            }
        }

        return type.ToString();
    }
}
