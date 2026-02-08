using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;

namespace ServerlessKakeibo.Api.Application.IncomeItemCategory.Usecases;

public interface IGetIncomeItemCategoriesUseCase
{
    Task<IncomeItemCategoryListResult> ExecuteAsync(
        Guid userId,
        bool includeHidden,
        CancellationToken cancellationToken = default);
}
