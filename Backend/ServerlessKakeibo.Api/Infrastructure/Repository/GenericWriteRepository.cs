using Microsoft.EntityFrameworkCore;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Infrastructure.Repository;

/// <summary>
/// 汎用書き込みリポジトリ実装
/// </summary>
public class GenericWriteRepository<T> : IGenericWriteRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _dbSet;

    public GenericWriteRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// エンティティを追加
    /// </summary>
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // 作成日時・更新日時の設定
        var now = DateTimeOffset.UtcNow;
        entity.CreatedAt = now;
        entity.UpdatedAt = now;

        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// 複数のエンティティを追加
    /// </summary>
    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var now = DateTimeOffset.UtcNow;
        foreach (var entity in entities)
        {
            entity.CreatedAt = now;
            entity.UpdatedAt = now;
        }

        await _dbSet.AddRangeAsync(entities, cancellationToken);
    }

    /// <summary>
    /// エンティティを更新
    /// </summary>
    public Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // 更新日時の設定
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    /// <summary>
    /// 複数のエンティティを更新
    /// </summary>
    public Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null)
            throw new ArgumentNullException(nameof(entities));

        var now = DateTimeOffset.UtcNow;
        foreach (var entity in entities)
        {
            entity.UpdatedAt = now;
        }

        _dbSet.UpdateRange(entities);
        return Task.CompletedTask;
    }

    /// <summary>
    /// エンティティを論理削除
    /// </summary>
    public async Task<T> SoftDeleteAsync(Guid id, Guid deletedBy, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .IgnoreQueryFilters() // 論理削除済みも含めて検索
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Entity with ID {id} not found.");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.UpdatedBy = deletedBy;

        return entity;
    }

    /// <summary>
    /// エンティティを物理削除
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (entity == null)
            throw new KeyNotFoundException($"Entity with ID {id} not found.");

        _dbSet.Remove(entity);
    }

    /// <summary>
    /// エンティティを物理削除（エンティティ指定）
    /// </summary>
    public Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _dbSet.Remove(entity);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 変更を保存
    /// </summary>
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
