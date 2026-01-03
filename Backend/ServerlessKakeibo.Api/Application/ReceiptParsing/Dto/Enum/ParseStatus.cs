namespace ServerlessKakeibo.Api.Application.ReceiptParsing.Dto.Enum;

/// <summary>
/// 解析ステータス
/// </summary>
public enum ParseStatus
{
    /// <summary>
    /// 完全解析成功
    /// </summary>
    Complete = 0,

    /// <summary>
    /// 部分的解析成功
    /// </summary>
    Partial = 1,

    /// <summary>
    /// 低信頼度
    /// </summary>
    LowConfidence = 2,

    /// <summary>
    /// 解析失敗
    /// </summary>
    Failed = 3
}
