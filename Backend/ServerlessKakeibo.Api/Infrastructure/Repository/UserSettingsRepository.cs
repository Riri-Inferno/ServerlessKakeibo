using Microsoft.EntityFrameworkCore;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Infrastructure.Repository;

/// <summary>
/// ユーザー設定リポジトリ
/// </summary>
public class UserSettingsRepository : IUserSettingsRepository
{
    private readonly ApplicationDbContext _context;

    public UserSettingsRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// ユーザーIDから設定を取得(ユーザー情報も含む)
    /// </summary>
    public async Task<UserSettingsEntity?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserSettings
            .AsNoTracking()
            .Include(us => us.User)
            .Where(us => us.UserId == userId && !us.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 設定を取得、存在しなければデフォルト設定を作成
    /// </summary>
    public async Task<UserSettingsEntity> GetOrCreateAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        // 既存の設定を取得
        var settings = await _context.UserSettings
            .Include(us => us.User)
            .Where(us => us.UserId == userId && !us.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (settings != null)
        {
            return settings;
        }

        // 存在しない場合はデフォルト設定を作成
        var newSettings = new UserSettingsEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ClosingDay = null, // 月末締め
            TimeZone = "Asia/Tokyo",
            CurrencyCode = "JPY",
            DisplayNameOverride = null,
            TenantId = tenantId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        _context.UserSettings.Add(newSettings);
        await _context.SaveChangesAsync(cancellationToken);

        // User情報を含めて返すため再取得
        return await _context.UserSettings
            .Include(us => us.User)
            .Where(us => us.Id == newSettings.Id)
            .FirstAsync(cancellationToken);
    }

    /// <summary>
    /// 設定を更新
    /// </summary>
    public async Task UpdateAsync(
        UserSettingsEntity settings,
        CancellationToken cancellationToken = default)
    {
        settings.UpdatedAt = DateTimeOffset.UtcNow;
        settings.UpdatedBy = settings.UserId;

        _context.UserSettings.Update(settings);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
