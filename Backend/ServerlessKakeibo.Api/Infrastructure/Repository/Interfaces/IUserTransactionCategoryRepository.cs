using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

/// <summary>
/// ユーザー取引カテゴリリポジトリ
/// </summary>
public interface IUserTransactionCategoryRepository
{
    /// <summary>
    /// ユーザー設定IDでカテゴリ一覧を取得（非表示含む）
    /// </summary>
    Task<List<UserTransactionCategoryEntity>> GetByUserSettingsIdAsync(
        Guid userSettingsId,
        bool includeHidden = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// カテゴリIDでカテゴリを取得
    /// </summary>
    Task<UserTransactionCategoryEntity?> GetByIdAsync(
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
}
