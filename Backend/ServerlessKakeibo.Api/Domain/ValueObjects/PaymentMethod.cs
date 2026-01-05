namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// 支払方法の値オブジェクト
/// </summary>
public record PaymentMethod
{
    public string Value { get; init; }

    private PaymentMethod(string value)
    {
        Value = value;
    }

    // 定義済みの支払方法
    public static PaymentMethod Cash => new("現金");
    public static PaymentMethod CreditCard => new("クレジットカード");
    public static PaymentMethod DebitCard => new("デビットカード");
    public static PaymentMethod ElectronicMoney => new("電子マネー");
    public static PaymentMethod BankTransfer => new("銀行振込");
    public static PaymentMethod QrCode => new("QRコード決済");
    public static PaymentMethod Other => new("その他");

    /// <summary>
    /// 文字列から生成
    /// </summary>
    public static PaymentMethod FromString(string value)
    {
        return value switch
        {
            "現金" or "Cash" => Cash,
            "クレジットカード" or "CreditCard" => CreditCard,
            "デビットカード" or "DebitCard" => DebitCard,
            "電子マネー" or "ElectronicMoney" => ElectronicMoney,
            "銀行振込" or "BankTransfer" => BankTransfer,
            "QRコード決済" or "QrCode" => QrCode,
            _ => new PaymentMethod(value)
        };
    }

    public override string ToString() => Value;
}
