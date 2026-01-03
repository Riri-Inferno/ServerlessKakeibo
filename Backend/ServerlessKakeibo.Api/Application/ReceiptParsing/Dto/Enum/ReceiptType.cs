namespace ServerlessKakeibo.Api.Application.ReceiptParsing.Dto.Enum;

/// <summary>
/// 領収書タイプ
/// </summary>
public enum ReceiptType
{
    /// <summary>
    /// 不明
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 領収書
    /// </summary>
    Receipt,

    /// <summary>
    /// 請求書
    /// </summary>
    Invoice,

    /// <summary>
    /// クレジットカード利用明細書
    /// </summary>
    CreditCardSlip
}
