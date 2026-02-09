using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

/// <summary>
/// ユーザー給与項目カテゴリリポジトリ
/// </summary>
public interface IUserIncomeItemCategoryRepository
{
    /// <summary>
    /// ユーザー設定IDでカテゴリ一覧を取得（非表示含む）
    /// </summary>
    Task<List<UserIncomeItemCategoryEntity>> GetByUserSettingsIdAsync(
        Guid userSettingsId,
        bool includeHidden = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// カテゴリIDでカテゴリを取得
    /// </summary>
    Task<UserIncomeItemCategoryEntity?> GetByIdAsync(
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
    Task<List<UserIncomeItemCategoryEntity>> GetByIdsAsync(
        List<Guid> ids,
        CancellationToken cancellationToken = default);
}
