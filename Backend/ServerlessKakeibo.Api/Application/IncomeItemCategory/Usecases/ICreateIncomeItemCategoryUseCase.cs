using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.IncomeItemCategory.Usecases;

public interface ICreateIncomeItemCategoryUseCase
{
    Task<IncomeItemCategoryResult> ExecuteAsync(
        Guid userId,
        CreateIncomeItemCategoryRequest request,
        CancellationToken cancellationToken = default);
}
