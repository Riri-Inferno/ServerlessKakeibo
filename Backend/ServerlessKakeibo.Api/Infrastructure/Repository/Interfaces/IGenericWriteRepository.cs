using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

/// <summary>
/// 汎用書き込みリポジトリインターフェース
/// </summary>
/// <typeparam name="T">BaseEntityを継承したエンティティ</typeparam>
public interface IGenericWriteRepository<T> where T : BaseEntity
{
    /// <summary>
    /// エンティティを追加
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 複数のエンティティを追加
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティを更新
    /// </summary>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 複数のエンティティを更新
    /// </summary>
    Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティを論理削除
    /// </summary>
    Task<T> SoftDeleteAsync(Guid id, Guid deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティを物理削除
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティを物理削除（エンティティ指定）
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 変更を保存
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
