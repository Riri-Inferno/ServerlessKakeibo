using ServerlessKakeibo.Api.Application.registReceiptDetails.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.registReceiptDetails.Usecases;

/// <summary>
/// 領収書詳細保存ユースケース
/// </summary>
public interface IregistReceiptDetailsUseCase
{
    /// <summary>
    /// 領収書解析結果を取引として保存
    /// </summary>
    Task<SaveTransactionResultDto> ExecuteSaveAsync(
        SaveReceiptParseResultRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);
}
