using ServerlessKakeibo.Api.Application.TransactionExport.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.TransactionExport.Usecases;

/// <summary>
/// 取引エクスポートユースケース
/// </summary>
public interface ITransactionExportUseCase
{
    /// <summary>
    /// 取引データをCSV+Zipでエクスポート
    /// </summary>
    /// <param name="request">エクスポート条件</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>エクスポート結果（Zipバイナリ含む）</returns>
    Task<TransactionExportResult> ExecuteAsync(
        ExportTransactionsRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);
}
