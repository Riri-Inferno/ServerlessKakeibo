using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

/// <summary>
/// ユーザー商品カテゴリリポジトリ
/// </summary>
public interface IUserItemCategoryRepository
{
    /// <summary>
    /// ユーザー設定IDでカテゴリ一覧を取得（非表示含む）
    /// </summary>
    Task<List<UserItemCategoryEntity>> GetByUserSettingsIdAsync(
        Guid userSettingsId,
        bool includeHidden = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// カテゴリIDでカテゴリを取得
    /// </summary>
    Task<UserItemCategoryEntity?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// マスタ由来のカテゴリを全削除し、マスタから再コピー
    /// </summary>
    Task ResetToMasterAsync(
        Guid userSettingsId,
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 複数IDでカテゴリを取得
    /// </summary>
    Task<List<UserItemCategoryEntity>> GetByIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default);
}
