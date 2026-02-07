using ServerlessKakeibo.Api.Application.ItemCategory.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.ItemCategory.Usecases;

public interface ICreateItemCategoryUseCase
{
    Task<ItemCategoryResult> ExecuteAsync(
        Guid userId,
        CreateItemCategoryRequest request,
        CancellationToken cancellationToken = default);
}
