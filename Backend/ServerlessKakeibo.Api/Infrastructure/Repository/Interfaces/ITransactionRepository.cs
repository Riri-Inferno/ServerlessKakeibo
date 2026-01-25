using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

public interface ITransactionRepository
{
    /// <summary>
    /// 取引詳細を取得(関連データを含む)
    /// </summary>
    Task<TransactionEntity?> GetDetailByIdAsync(
        Guid id,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// 取引一覧を取得(ページング対応)
    /// </summary>
    Task<(List<TransactionEntity> Items, int TotalCount)> GetPagedListAsync(
        Guid userId,
        int page,
        int pageSize,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        TransactionCategory? category = null,
        string? payee = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        TransactionType? type = null,
        CancellationToken ct = default);

    /// <summary>
    /// 取引を更新用に取得(関連データを含む)
    /// </summary>
    Task<TransactionEntity?> GetForUpdateAsync(
        Guid id,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// 取引に関連する全データを一括で論理削除
    /// </summary>
    Task SoftDeleteWithRelatedDataAsync(
        Guid transactionId,
        Guid userId,
        CancellationToken ct = default);

    /// <summary>
    /// 月次サマリーを取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="year">対象年</param>
    /// <param name="month">対象月</param>
    /// <param name="ct">キャンセレーショントークン</param>
    /// <returns>収入合計、支出合計、各カテゴリ別集計</returns>
    Task<(decimal TotalIncome, decimal TotalExpense, Dictionary<TransactionCategory, (decimal Amount, int Count)> ExpenseByCategory)>
        GetMonthlySummaryAsync(
            Guid userId,
            int year,
            int month,
            CancellationToken ct = default);

    /// <summary>
    /// エクスポート用に取引一覧を取得（ページング無視、全件取得）
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="startDate">開始日</param>
    /// <param name="endDate">終了日</param>
    /// <param name="category">カテゴリフィルタ</param>
    /// <param name="payee">受取者フィルタ</param>
    /// <param name="minAmount">最小金額</param>
    /// <param name="maxAmount">最大金額</param>
    /// <param name="type">取引種別フィルタ</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>フィルタ条件に一致する全取引（Items, Taxes, ShopDetail含む）</returns>
    Task<List<TransactionEntity>> GetAllForExportAsync(
        Guid userId,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        TransactionCategory? category = null,
        string? payee = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        TransactionType? type = null,
        CancellationToken ct = default);
}
