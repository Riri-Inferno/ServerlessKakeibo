namespace ServerlessKakeibo.Api.Application.ReceiptParsing.Dto.Enum;

/// <summary>
/// LLMが判定した支払方法
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// 不明
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 現金
    /// </summary>
    Cash,

    /// <summary>
    /// クレジットカード
    /// </summary>
    CreditCard,

    /// <summary>
    /// デビットカード
    /// </summary>
    DebitCard,

    /// <summary>
    /// 電子マネー（交通系IC等）
    /// </summary>
    ElectronicMoney,

    /// <summary>
    /// QRコード決済（PayPay, 楽天Pay 等）
    /// </summary>
    QRCodePayment,

    /// <summary>
    /// 銀行振込
    /// </summary>
    BankTransfer,

    /// <summary>
    /// その他
    /// </summary>
    Other
}
