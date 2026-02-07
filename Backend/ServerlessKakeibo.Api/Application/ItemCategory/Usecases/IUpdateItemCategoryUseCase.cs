using ServerlessKakeibo.Api.Application.ItemCategory.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.ItemCategory.Usecases;

public interface IUpdateItemCategoryUseCase
{
    Task<ItemCategoryResult> ExecuteAsync(
        Guid userId,
        Guid categoryId,
        UpdateItemCategoryRequest request,
        CancellationToken cancellationToken = default);
}
