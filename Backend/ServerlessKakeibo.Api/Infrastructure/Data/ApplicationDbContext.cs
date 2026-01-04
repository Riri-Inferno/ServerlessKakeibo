using Microsoft.EntityFrameworkCore;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Infrastructure.Data;


/// <summary>
/// 永続化用の EF Core DbContext
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // 永続化エンティティを DbSet として登録
    public DbSet<ReceiptEntity> Receipts { get; set; } = default!;

    /// <summary>
    /// モデル作成時の追加設定
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // すべての BaseEntity 継承クラスに xmin による楽観ロックを設定
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // xmin を RowVersion として設定
                modelBuilder.Entity(entityType.ClrType)
                    .Property<uint>("RowVersion")
                    .HasColumnName("xmin")
                    .HasColumnType("xid")
                    .ValueGeneratedOnAddOrUpdate()
                    .IsConcurrencyToken();

                // 論理削除のグローバルフィルター
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var notDeleted = System.Linq.Expressions.Expression.Not(property);
                var lambda = System.Linq.Expressions.Expression.Lambda(notDeleted, parameter);

                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(lambda);
            }
        }
    }
}

/// <summary>
/// 仮置き
/// </summary>
public class ReceiptEntity : BaseEntity
{
    public int hoge { get; set; }
}
