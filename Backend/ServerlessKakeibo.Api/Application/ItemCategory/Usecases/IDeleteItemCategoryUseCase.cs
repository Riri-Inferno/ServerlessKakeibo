using ServerlessKakeibo.Api.Application.ItemCategory.Dto;

namespace ServerlessKakeibo.Api.Application.ItemCategory.Usecases;

public interface IDeleteItemCategoryUseCase
{
    Task<ItemCategoryResult> ExecuteAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken cancellationToken = default);
}
