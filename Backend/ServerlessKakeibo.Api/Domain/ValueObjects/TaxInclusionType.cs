using System.ComponentModel;

namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// 税の扱い（税金の計算方法）
/// </summary>
public enum TaxInclusionType
{
    /// <summary>
    /// 不明・判定不能
    /// </summary>
    [Description("不明")]
    Unknown = 0,

    /// <summary>
    /// 外税（小計 + 税額 = 合計）
    /// Tax-exclusive: Total = Subtotal + Tax
    /// </summary>
    [Description("外税")]
    Exclusive = 1,

    /// <summary>
    /// 内税（小計 = 合計、税額は内包）
    /// Tax-inclusive: Total = Subtotal (tax already included)
    /// </summary>
    [Description("内税")]
    Inclusive = 2,

    /// <summary>
    /// 非課税（税額なし）
    /// No tax applied
    /// </summary>
    [Description("非課税")]
    NoTax = 3
}

/// <summary>
/// TaxInclusionType 拡張メソッド
/// </summary>
public static class TaxInclusionTypeExtensions
{
    /// <summary>
    /// 日本語名を取得
    /// </summary>
    public static string ToJapanese(this TaxInclusionType type)
    {
        var enumType = typeof(TaxInclusionType);
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
    public static TaxInclusionType? FromJapanese(string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return null;

        return typeName switch
        {
            "内税" => TaxInclusionType.Inclusive,
            "外税" => TaxInclusionType.Exclusive,
            "非課税" => TaxInclusionType.NoTax,
            "不明" => TaxInclusionType.Unknown,
            _ => null
        };
    }
}
