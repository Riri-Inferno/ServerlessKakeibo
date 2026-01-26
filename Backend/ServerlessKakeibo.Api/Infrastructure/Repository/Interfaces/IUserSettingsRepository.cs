using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

/// <summary>
/// ユーザー設定リポジトリのインターフェース
/// </summary>
public interface IUserSettingsRepository
{
    /// <summary>
    /// ユーザーIDから設定を取得(ユーザー情報も含む)
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>設定エンティティ(存在しない場合はnull)</returns>
    Task<UserSettingsEntity?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 設定を取得、存在しなければデフォルト設定を作成
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="tenantId">テナントID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>設定エンティティ</returns>
    Task<UserSettingsEntity> GetOrCreateAsync(
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 設定を更新
    /// </summary>
    /// <param name="settings">更新する設定エンティティ</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    Task UpdateAsync(
        UserSettingsEntity settings,
        CancellationToken cancellationToken = default);
}
