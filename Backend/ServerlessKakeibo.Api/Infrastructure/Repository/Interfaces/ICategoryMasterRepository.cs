using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

/// <summary>
/// カテゴリマスタリポジトリ
/// </summary>
public interface ICategoryMasterRepository
{
    /// <summary>
    /// 取引カテゴリマスタを全件取得
    /// </summary>
    Task<List<TransactionCategoryMasterEntity>> GetAllTransactionCategoryMastersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 商品カテゴリマスタを全件取得
    /// </summary>
    Task<List<ItemCategoryMasterEntity>> GetAllItemCategoryMastersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 給与項目カテゴリマスタを全件取得
    /// </summary>
    Task<List<IncomeItemCategoryMasterEntity>> GetAllIncomeItemCategoryMastersAsync(
        CancellationToken cancellationToken = default);
}
