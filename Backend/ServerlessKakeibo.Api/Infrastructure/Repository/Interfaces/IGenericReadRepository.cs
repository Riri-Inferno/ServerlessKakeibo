using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

/// <summary>
/// 汎用読み取りリポジトリインターフェース
/// </summary>
/// <typeparam name="T">BaseEntityを継承したエンティティ</typeparam>
public interface IGenericReadRepository<T> where T : BaseEntity
{
    /// <summary>
    /// IDでエンティティを取得
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// IDでエンティティを取得（削除済みも含む）
    /// </summary>
    Task<T?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// すべてのエンティティを取得
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 条件に一致するエンティティを取得
    /// </summary>
    Task<IReadOnlyList<T>> FindAsync(
        System.Linq.Expressions.Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// エンティティの存在確認
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 条件に一致するエンティティの件数を取得
    /// </summary>
    Task<int> CountAsync(
        System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);
}
