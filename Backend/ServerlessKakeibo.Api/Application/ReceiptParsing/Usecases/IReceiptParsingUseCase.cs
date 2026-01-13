using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.ReceiptParsing.UseCase;

/// <summary>
/// 領収書解析ユースケースインタフェース
/// </summary>
public interface IReceiptParsingUseCase
{
    /// <summary>
    /// 領収書解析
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<ReceiptParseResult> ExcuteParseAsync(ReceiptParseRequest request, Guid userId);
}
