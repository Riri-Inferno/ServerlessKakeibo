using ServerlessKakeibo.Api.Application.ItemCategory.Dto;

namespace ServerlessKakeibo.Api.Application.ItemCategory.Usecases;

public interface IResetItemCategoriesToMasterUseCase
{
    Task<ItemCategoryListResult> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
