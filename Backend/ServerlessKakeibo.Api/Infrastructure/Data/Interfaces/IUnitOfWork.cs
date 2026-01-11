using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;

/// <summary>
/// Unit of Work パターンによるトランザクション管理
/// 使用禁止：トランザクションヘルパーに置き換えて消す。
/// </summary>
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// トランザクションを開始
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// トランザクションをコミット
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// トランザクションをロールバック
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 変更を保存（トランザクション外での保存）
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 読み取りリポジトリ取得
    /// </summary>
    IGenericReadRepository<TEntity> ReadRepository<TEntity>() where TEntity : BaseEntity;

    /// <summary>
    /// 書き込みリポジトリ取得
    /// </summary>
    IGenericWriteRepository<TEntity> WriteRepository<TEntity>() where TEntity : BaseEntity;

    /// <summary>
    /// トランザクションスコープ内で処理を実行
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<Task<TResult>> operation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// トランザクションスコープ内で処理を実行（戻り値なし）
    /// </summary>
    Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default);
}
