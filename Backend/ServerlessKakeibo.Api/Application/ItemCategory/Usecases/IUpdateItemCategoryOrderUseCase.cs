using ServerlessKakeibo.Api.Application.ItemCategory.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.ItemCategory.Usecases;

/// <summary>
/// 商品カテゴリ並び順一括更新ユースケース
/// </summary>
public interface IUpdateItemCategoryOrderUseCase
{
    Task<ItemCategoryListResult> ExecuteAsync(
        Guid userId,
        UpdateItemCategoryOrderRequest request,
        CancellationToken cancellationToken = default);
}
