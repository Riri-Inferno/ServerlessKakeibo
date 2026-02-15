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
        string? payer = null,
        string? payee = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        TransactionType? type = null,
        Guid? userTransactionCategoryId = null,
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
    Task<(
        decimal TotalIncome,
        decimal TotalExpense,
        Dictionary<TransactionCategory, (decimal Amount, int Count)> IncomeByCategory,
        Dictionary<TransactionCategory, (decimal Amount, int Count)> ExpenseByCategory)>
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

    /// <summary>
    /// 全カテゴリの支出内訳を取得（TopN制限なし）
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="year">対象年</param>
    /// <param name="month">対象月</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>カテゴリ別の金額と件数</returns>
    Task<Dictionary<TransactionCategory, (decimal Amount, int Count)>> GetAllCategoryExpensesAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken ct = default);

    /// <summary>
    /// 指定月の最高額支出取引を取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="year">対象年</param>
    /// <param name="month">対象月</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>最高額の取引（支出のみ）</returns>
    Task<TransactionEntity?> GetMaxExpenseTransactionAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken ct = default);

    /// <summary>
    /// 指定月の最も頻度の高いカテゴリを取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="year">対象年</param>
    /// <param name="month">対象月</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>カテゴリ、件数、合計金額</returns>
    Task<(TransactionCategory Category, int Count, decimal TotalAmount)?> GetMostFrequentCategoryAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken ct = default);

    /// <summary>
    /// 指定月の支出があった日数を取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="year">対象年</param>
    /// <param name="month">対象月</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>支出があった日数</returns>
    Task<int> GetDaysWithExpenseAsync(
        Guid userId,
        int year,
        int month,
        CancellationToken ct = default);

    /// <summary>
    /// 複数月の月次サマリーを一括取得（月次推移用）
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="monthlyRanges">取得する年月のリスト</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>各月の収入・支出・収支</returns>
    Task<List<(int Year, int Month, decimal Income, decimal Expense, decimal Balance)>> GetMonthlyAggregatesAsync(
        Guid userId,
        List<(int Year, int Month)> monthlyRanges,
        CancellationToken ct = default);

    /// <summary>
    /// ユーザーの全取引に紐づく画像URLを一括取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>画像URLのリスト</returns>
    Task<List<string>> GetAllReceiptImageUrlsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// ユーザーの全取引データを一括で論理削除
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="ct">キャンセルトークン</param>
    /// <returns>削除件数(Transactions, Items, Taxes, Shops)</returns>
    Task<(int Transactions, int Items, int Taxes, int Shops)> SoftDeleteAllUserTransactionsAsync(
        Guid userId,
        CancellationToken ct = default);
}
