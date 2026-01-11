namespace ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;

/// <summary>
/// トランザクション管理を提供するヘルパーインターフェース
/// </summary>
public interface ITransactionHelper
{
    /// <summary>
    /// トランザクション内で非同期処理を実行する
    /// </summary>
    /// <param name="action">実行する非同期アクション</param>
    /// <returns>処理完了を表すタスク</returns>
    /// <exception cref="InvalidOperationException">
    /// 同時実行の競合が発生した場合、またはトランザクションの実行中にエラーが発生した場合
    /// </exception>
    /// <remarks>
    /// このメソッドは自動的にトランザクションを開始し、成功時にコミット、失敗時にロールバックする。
    /// Entity Framework Core の ExecutionStrategy を使用して再試行可能なエラーに対応する。
    /// </remarks>
    Task ExecuteInTransactionAsync(Func<Task> action);

    /// <summary>
    /// トランザクション内で非同期処理を実行し、結果を返する
    /// </summary>
    /// <typeparam name="T">戻り値の型</typeparam>
    /// <param name="func">実行する非同期関数</param>
    /// <returns>関数の実行結果を含むタスク</returns>
    /// <exception cref="InvalidOperationException">
    /// 同時実行の競合が発生した場合、またはトランザクションの実行中にエラーが発生した場合
    /// </exception>
    /// <remarks>
    /// このメソッドは自動的にトランザクションを開始し、成功時にコミット、失敗時にロールバックする。
    /// Entity Framework Core の ExecutionStrategy を使用して再試行可能なエラーに対応する。
    /// </remarks>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> func);
}
