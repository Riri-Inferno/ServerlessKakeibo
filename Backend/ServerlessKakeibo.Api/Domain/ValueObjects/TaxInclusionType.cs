namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// 税の扱い（税金の計算方法）
/// </summary>
public enum TaxInclusionType
{
    /// <summary>
    /// 不明・判定不能
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 外税（小計 + 税額 = 合計）
    /// Tax-exclusive: Total = Subtotal + Tax
    /// </summary>
    Exclusive = 1,

    /// <summary>
    /// 内税（小計 = 合計、税額は内包）
    /// Tax-inclusive: Total = Subtotal (tax already included)
    /// </summary>
    Inclusive = 2,

    /// <summary>
    /// 非課税（税額なし）
    /// No tax applied
    /// </summary>
    NoTax = 3
}
