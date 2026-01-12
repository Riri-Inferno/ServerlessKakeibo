using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

/// <summary>
/// 外部ログイン情報リポジトリインターフェース
/// </summary>
public interface IUserExternalLoginRepository
{
    /// <summary>
    /// プロバイダーとキーで外部ログイン情報を取得
    /// </summary>
    Task<UserExternalLoginEntity?> GetByProviderAsync(
        AuthProvider providerName,
        string providerKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 外部ログイン情報を作成
    /// </summary>
    Task<UserExternalLoginEntity> CreateAsync(
        UserExternalLoginEntity entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ユーザーIDで外部ログイン情報の一覧を取得
    /// </summary>
    Task<List<UserExternalLoginEntity>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
