using Microsoft.EntityFrameworkCore;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Infrastructure.Repository;

/// <summary>
/// 外部ログイン情報リポジトリ
/// </summary>
public class UserExternalLoginRepository : IUserExternalLoginRepository
{
    private readonly ApplicationDbContext _context;

    public UserExternalLoginRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// プロバイダーとキーで外部ログイン情報を取得
    /// </summary>
    public async Task<UserExternalLoginEntity?> GetByProviderAsync(
        AuthProvider providerName,
        string providerKey,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserExternalLogins
            .AsNoTracking()
            .Include(ue => ue.User)
            .Where(ue => ue.ProviderName == providerName
                && ue.ProviderKey == providerKey
                && !ue.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 外部ログイン情報を作成
    /// </summary>
    public async Task<UserExternalLoginEntity> CreateAsync(
        UserExternalLoginEntity entity,
        CancellationToken cancellationToken = default)
    {
        _context.UserExternalLogins.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    /// <summary>
    /// ユーザーIDで外部ログイン情報の一覧を取得
    /// </summary>
    public async Task<List<UserExternalLoginEntity>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserExternalLogins
            .AsNoTracking()
            .Where(ue => ue.UserId == userId && !ue.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}
