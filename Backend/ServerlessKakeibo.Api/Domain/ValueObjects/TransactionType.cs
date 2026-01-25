using System.ComponentModel;

namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// 取引種別（収入/支出）
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// 支出
    /// </summary>
    [Description("支出")]
    Expense = 0,

    /// <summary>
    /// 収入
    /// </summary>
    [Description("収入")]
    Income = 1
}

/// <summary>
/// TransactionType 拡張メソッド
/// </summary>
public static class TransactionTypeExtensions
{
    /// <summary>
    /// 日本語名を取得
    /// </summary>
    public static string ToJapanese(this TransactionType type)
    {
        var enumType = typeof(TransactionType);
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

    /// <summary>
    /// 日本語名からEnumに変換
    /// </summary>
    public static TransactionType? FromJapanese(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        return typeName switch
        {
            "支出" => TransactionType.Expense,
            "収入" => TransactionType.Income,
            _ => null
        };
    }
}
