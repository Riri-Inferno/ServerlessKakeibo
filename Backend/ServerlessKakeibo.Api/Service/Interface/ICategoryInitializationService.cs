namespace ServerlessKakeibo.Api.Service.Interface;

/// <summary>
/// カテゴリ初期化サービス
/// ユーザー登録時にマスタカテゴリをコピーする
/// </summary>
public interface ICategoryInitializationService
{
    /// <summary>
    /// ユーザー設定に対してマスタカテゴリを全件コピーする
    /// </summary>
    /// <param name="userSettingsId">ユーザー設定ID</param>
    /// <param name="userId">ユーザーID（CreatedBy/UpdatedBy用）</param>
    /// <param name="tenantId">テナントID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    Task InitializeUserCategoriesAsync(
        Guid userSettingsId,
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}
