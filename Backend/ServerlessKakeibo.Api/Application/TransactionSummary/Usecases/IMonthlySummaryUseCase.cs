using ServerlessKakeibo.Api.Application.TransactionSummary.Dto;

namespace ServerlessKakeibo.Api.Application.TransactionSummary.Usecases;

/// <summary>
/// 月次サマリーユースケース
/// </summary>
public interface IMonthlySummaryUseCase
{
    /// <summary>
    /// 月次サマリーを取得
    /// </summary>
    /// <param name="year">対象年</param>
    /// <param name="month">対象月</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>月次サマリー結果</returns>
    Task<MonthlySummaryResult> GetMonthlySummaryAsync(
        int year,
        int month,
        Guid userId,
        CancellationToken cancellationToken = default);
}
