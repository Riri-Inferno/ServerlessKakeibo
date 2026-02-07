using ServerlessKakeibo.Api.Application.ItemCategory.Dto;

namespace ServerlessKakeibo.Api.Application.ItemCategory.Usecases;

public interface IRestoreItemCategoryUseCase
{
    Task<ItemCategoryResult> ExecuteAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken cancellationToken = default);
}
