using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;

namespace ServerlessKakeibo.Api.Application.IncomeItemCategory.Usecases;

public interface IRestoreIncomeItemCategoryUseCase
{
    Task<IncomeItemCategoryResult> ExecuteAsync(
        Guid userId,
        Guid categoryId,
        CancellationToken cancellationToken = default);
}
