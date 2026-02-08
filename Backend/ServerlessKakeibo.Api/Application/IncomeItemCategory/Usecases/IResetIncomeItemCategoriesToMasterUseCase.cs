using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;

namespace ServerlessKakeibo.Api.Application.IncomeItemCategory.Usecases;

public interface IResetIncomeItemCategoriesToMasterUseCase
{
    Task<IncomeItemCategoryListResult> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
