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

    // 永続化エンティティを登録
    public DbSet<UserEntity> Users { get; set; } = default!;
    public DbSet<TransactionEntity> Transactions { get; set; } = default!;
    public DbSet<TransactionItemEntity> TransactionItems { get; set; } = default!;
    public DbSet<TaxDetailEntity> TaxDetails { get; set; } = default!;
    public DbSet<ShopDetailEntity> ShopDetails { get; set; } = default!;

    /// <summary>
    /// 保存前の自動処理
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // BaseEntity継承エンティティの自動設定
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            // すでに設定済みの場合はスキップ
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)  // 未設定の場合のみ
                {
                    var now = DateTimeOffset.UtcNow;
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdatedAt = now;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity.UpdatedAt == default ||
                    entry.Entity.UpdatedAt < entry.Entity.CreatedAt)  // 不正な値の場合のみ
                {
                    entry.Entity.UpdatedAt = DateTimeOffset.UtcNow;
                }

                entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
                entry.Property(nameof(BaseEntity.CreatedBy)).IsModified = false;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region DateTimeOffset Conversion

        // すべての DateTimeOffset 型プロパティを UTC に正規化
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset) ||
                    property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(
                        new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTimeOffset, DateTimeOffset>(
                            v => v.ToUniversalTime(),      // 書き込み時
                            v => v.ToUniversalTime()       // 読み込み時
                        )
                    );
                }
            }
        }

        #endregion

        #region indexes

        // UserEntity
        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("\"Email\" IS NOT NULL");

        modelBuilder.Entity<UserEntity>()
            .HasIndex(u => u.TenantId);

        // TransactionEntity
        modelBuilder.Entity<TransactionEntity>()
            .HasIndex(t => t.UserId);

        modelBuilder.Entity<TransactionEntity>()
            .HasIndex(t => t.TransactionDate);

        modelBuilder.Entity<TransactionEntity>()
            .HasIndex(t => new { t.UserId, t.TransactionDate })
            .HasDatabaseName("IX_Transactions_UserId_TransactionDate");

        modelBuilder.Entity<TransactionEntity>()
            .HasIndex(t => t.TenantId);

        modelBuilder.Entity<TransactionEntity>()
            .HasIndex(t => t.IsDeleted);

        modelBuilder.Entity<TransactionItemEntity>()
            .HasIndex(i => i.TransactionId);

        // TaxDetailEntity
        modelBuilder.Entity<TaxDetailEntity>()
            .HasIndex(td => td.TransactionId);

        // ShopDetailEntity
        modelBuilder.Entity<ShopDetailEntity>()
            .HasIndex(sd => sd.TransactionId)
            .IsUnique();

        modelBuilder.Entity<ShopDetailEntity>()
            .HasIndex(sd => sd.Name);
        #endregion

        #region relationships
        // Transaction - User のリレーション
        modelBuilder.Entity<TransactionEntity>()
            .HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Transaction - TransactionItem のリレーション
        modelBuilder.Entity<TransactionItemEntity>()
            .HasOne(i => i.Transaction)
            .WithMany(t => t.Items)
            .HasForeignKey(i => i.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Transaction - TaxDetail のリレーション
        modelBuilder.Entity<TaxDetailEntity>()
            .HasOne(td => td.Transaction)
            .WithMany(t => t.Taxes)
            .HasForeignKey(td => td.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Transaction - ShopDetail の1対1リレーション
        modelBuilder.Entity<ShopDetailEntity>()
            .HasOne(sd => sd.Transaction)
            .WithOne(t => t.ShopDetail)
            .HasForeignKey<ShopDetailEntity>(sd => sd.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion

        #region settings

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

        #endregion
    }
}
