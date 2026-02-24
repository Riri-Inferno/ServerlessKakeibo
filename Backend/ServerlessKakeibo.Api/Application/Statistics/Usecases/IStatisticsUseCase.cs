using ServerlessKakeibo.Api.Application.Statistics.Dto;

namespace ServerlessKakeibo.Api.Application.Statistics.Usecases;

/// <summary>
/// 統計情報取得ユースケース
/// </summary>
public interface IStatisticsUseCase
{
    /// <summary>
    /// 前月比込みの月次サマリーを取得
    /// </summary>
    /// <param name="year">対象年</param>
    /// <param name="month">対象月</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>前月比込みサマリー</returns>
    Task<MonthlyComparisonResult> GetMonthlyComparisonAsync(
        int year,
        int month,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// カテゴリ別支出内訳を取得（全カテゴリ）
    /// </summary>
    /// <param name="year">対象年</param>
    /// <param name="month">対象月</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>全カテゴリの支出内訳</returns>
    Task<CategoryBreakdownResult> GetCategoryBreakdownAsync(
        int year,
        int month,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 月次推移を取得（直近N ヶ月）
    /// </summary>
    /// <param name="months">取得する月数</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>月次推移データ</returns>
    Task<MonthlyTrendResult> GetMonthlyTrendAsync(
        int months,
        Guid userId,
        int? targetYear = null,
        int? targetMonth = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 月次ハイライトを取得
    /// </summary>
    /// <param name="year">対象年</param>
    /// <param name="month">対象月</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>ハイライト情報</returns>
    Task<HighlightsResult> GetHighlightsAsync(
        int year,
        int month,
        Guid userId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// 取引データの日付範囲を取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>最古・最新の取引年月</returns>
    Task<DateRangeResult> GetDateRangeAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
