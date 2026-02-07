using ServerlessKakeibo.Api.Application.ItemCategory.Dto;

namespace ServerlessKakeibo.Api.Application.ItemCategory.Usecases;

public interface IGetItemCategoriesUseCase
{
    Task<ItemCategoryListResult> ExecuteAsync(
        Guid userId,
        bool includeHidden,
        CancellationToken cancellationToken = default);
}
