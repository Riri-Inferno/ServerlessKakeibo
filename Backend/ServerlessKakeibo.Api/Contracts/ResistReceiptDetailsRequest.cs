using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// 領収書詳細保存リクエスト
/// </summary>
public class ResistReceiptDetailsRequest
{
    /// <summary>
    /// 領収書解析結果
    /// </summary>
    public ReceiptParseResult ParseResult { get; set; } = default!;
}
