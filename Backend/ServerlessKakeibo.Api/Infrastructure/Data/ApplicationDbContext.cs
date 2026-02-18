using Microsoft.AspNetCore.Identity;
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
    // ユーザー
    public DbSet<UserEntity> Users { get; set; } = default!;

    // ユーザー外部認証用
    public DbSet<UserExternalLoginEntity> UserExternalLogins { get; set; } = default!;

    // 取引
    public DbSet<TransactionEntity> Transactions { get; set; } = default!;

    // 取引明細
    public DbSet<TransactionItemEntity> TransactionItems { get; set; } = default!;

    // 税情報
    public DbSet<TaxDetailEntity> TaxDetails { get; set; } = default!;

    // 店舗情報
    public DbSet<ShopDetailEntity> ShopDetails { get; set; } = default!;

    // ユーザー設定
    public DbSet<UserSettingsEntity> UserSettings { get; set; } = default!;

    // カテゴリマスタ
    public DbSet<TransactionCategoryMasterEntity> TransactionCategoryMasters { get; set; } = default!;
    public DbSet<ItemCategoryMasterEntity> ItemCategoryMasters { get; set; } = default!;
    public DbSet<IncomeItemCategoryMasterEntity> IncomeItemCategoryMasters { get; set; } = default!;

    // ユーザーカテゴリ
    public DbSet<UserTransactionCategoryEntity> UserTransactionCategories { get; set; } = default!;
    public DbSet<UserItemCategoryEntity> UserItemCategories { get; set; } = default!;
    public DbSet<UserIncomeItemCategoryEntity> UserIncomeItemCategories { get; set; } = default!;

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

        // UserExternalLoginEntity
        // プロバイダ（Googleなど）と、そのプロバイダ内でのユーザーIDの組み合わせでユニークにする
        modelBuilder.Entity<UserExternalLoginEntity>()
            .HasIndex(ue => new { ue.ProviderName, ue.ProviderKey })
            .IsUnique();

        // UserSettingsEntity 
        modelBuilder.Entity<UserSettingsEntity>()
            .HasIndex(us => us.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserSettings_UserId");

        modelBuilder.Entity<UserSettingsEntity>()
            .HasIndex(us => us.TenantId)
            .HasDatabaseName("IX_UserSettings_TenantId");

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

        // TransactionCategoryMasterEntity
        modelBuilder.Entity<TransactionCategoryMasterEntity>()
            .HasIndex(m => m.Code)
            .IsUnique();

        modelBuilder.Entity<TransactionCategoryMasterEntity>()
            .HasIndex(m => m.DisplayOrder);

        // ItemCategoryMasterEntity
        modelBuilder.Entity<ItemCategoryMasterEntity>()
            .HasIndex(m => m.Code)
            .IsUnique();

        modelBuilder.Entity<ItemCategoryMasterEntity>()
            .HasIndex(m => m.DisplayOrder);

        // IncomeItemCategoryMasterEntity
        modelBuilder.Entity<IncomeItemCategoryMasterEntity>()
            .HasIndex(m => m.Code)
            .IsUnique();

        modelBuilder.Entity<IncomeItemCategoryMasterEntity>()
            .HasIndex(m => m.DisplayOrder);

        // UserTransactionCategoryEntity
        modelBuilder.Entity<UserTransactionCategoryEntity>()
            .HasIndex(c => c.UserSettingsId);

        modelBuilder.Entity<UserTransactionCategoryEntity>()
            .HasIndex(c => new { c.UserSettingsId, c.DisplayOrder });

        modelBuilder.Entity<UserTransactionCategoryEntity>()
            .HasIndex(c => c.MasterCategoryId);

        modelBuilder.Entity<UserTransactionCategoryEntity>()
            .HasIndex(c => c.IsHidden);

        // UserItemCategoryEntity
        modelBuilder.Entity<UserItemCategoryEntity>()
            .HasIndex(c => c.UserSettingsId);

        modelBuilder.Entity<UserItemCategoryEntity>()
            .HasIndex(c => new { c.UserSettingsId, c.DisplayOrder });

        modelBuilder.Entity<UserItemCategoryEntity>()
            .HasIndex(c => c.MasterCategoryId);

        modelBuilder.Entity<UserItemCategoryEntity>()
            .HasIndex(c => c.IsHidden);

        // UserIncomeItemCategoryEntity
        modelBuilder.Entity<UserIncomeItemCategoryEntity>()
            .HasIndex(c => c.UserSettingsId);

        modelBuilder.Entity<UserIncomeItemCategoryEntity>()
            .HasIndex(c => new { c.UserSettingsId, c.DisplayOrder });

        modelBuilder.Entity<UserIncomeItemCategoryEntity>()
            .HasIndex(c => c.MasterCategoryId);

        modelBuilder.Entity<UserIncomeItemCategoryEntity>()
            .HasIndex(c => c.IsHidden);

        // TransactionItemEntity（新規）
        modelBuilder.Entity<TransactionItemEntity>()
            .HasIndex(i => i.UserItemCategoryId);

        modelBuilder.Entity<TransactionItemEntity>()
            .HasIndex(i => i.UserIncomeItemCategoryId);
        #endregion

        #region relationships
        // User - UserExternalLogin のリレーション (1対多)
        modelBuilder.Entity<UserExternalLoginEntity>()
            .HasOne(ue => ue.User)
            .WithMany(u => u.ExternalLogins)
            .HasForeignKey(ue => ue.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User - UserSettings のリレーション (1対1)
        modelBuilder.Entity<UserSettingsEntity>()
            .HasOne(us => us.User)
            .WithOne(u => u.Settings)
            .HasForeignKey<UserSettingsEntity>(us => us.UserId)
            .OnDelete(DeleteBehavior.Cascade);

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

        // UserSettings - UserTransactionCategory のリレーション
        modelBuilder.Entity<UserTransactionCategoryEntity>()
            .HasOne(c => c.UserSettings)
            .WithMany(us => us.UserTransactionCategories)
            .HasForeignKey(c => c.UserSettingsId)
            .OnDelete(DeleteBehavior.Cascade);

        // TransactionCategoryMaster - UserTransactionCategory のリレーション
        modelBuilder.Entity<UserTransactionCategoryEntity>()
            .HasOne(c => c.MasterCategory)
            .WithMany(m => m.UserCategories)
            .HasForeignKey(c => c.MasterCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // UserSettings - UserItemCategory のリレーション
        modelBuilder.Entity<UserItemCategoryEntity>()
            .HasOne(c => c.UserSettings)
            .WithMany(us => us.UserItemCategories)
            .HasForeignKey(c => c.UserSettingsId)
            .OnDelete(DeleteBehavior.Cascade);

        // ItemCategoryMaster - UserItemCategory のリレーション
        modelBuilder.Entity<UserItemCategoryEntity>()
            .HasOne(c => c.MasterCategory)
            .WithMany(m => m.UserCategories)
            .HasForeignKey(c => c.MasterCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // UserSettings - UserIncomeItemCategory のリレーション
        modelBuilder.Entity<UserIncomeItemCategoryEntity>()
            .HasOne(c => c.UserSettings)
            .WithMany(us => us.UserIncomeItemCategories)
            .HasForeignKey(c => c.UserSettingsId)
            .OnDelete(DeleteBehavior.Cascade);

        // IncomeItemCategoryMaster - UserIncomeItemCategory のリレーション
        modelBuilder.Entity<UserIncomeItemCategoryEntity>()
            .HasOne(c => c.MasterCategory)
            .WithMany(m => m.UserCategories)
            .HasForeignKey(c => c.MasterCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Transaction - UserTransactionCategory のリレーション
        modelBuilder.Entity<TransactionEntity>()
            .HasOne(t => t.UserTransactionCategory)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.UserTransactionCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // TransactionItem - UserItemCategory のリレーション（支出用）
        modelBuilder.Entity<TransactionItemEntity>()
            .HasOne(i => i.UserItemCategory)
            .WithMany(c => c.TransactionItems)
            .HasForeignKey(i => i.UserItemCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // TransactionItem - UserIncomeItemCategory のリレーション（収入用）
        modelBuilder.Entity<TransactionItemEntity>()
            .HasOne(i => i.UserIncomeItemCategory)
            .WithMany(c => c.TransactionItems)
            .HasForeignKey(i => i.UserIncomeItemCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        #endregion

        #region settings
        // UserSettingsEntity のデフォルト値設定
        modelBuilder.Entity<UserSettingsEntity>()
            .Property(us => us.TimeZone)
            .HasDefaultValue("Asia/Tokyo");

        modelBuilder.Entity<UserSettingsEntity>()
            .Property(us => us.CurrencyCode)
            .HasDefaultValue("JPY");

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
