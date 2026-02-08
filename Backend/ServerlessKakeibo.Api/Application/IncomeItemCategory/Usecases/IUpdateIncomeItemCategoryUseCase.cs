using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.IncomeItemCategory.Usecases;

public interface IUpdateIncomeItemCategoryUseCase
{
    Task<IncomeItemCategoryResult> ExecuteAsync(
        Guid userId,
        Guid categoryId,
        UpdateIncomeItemCategoryRequest request,
        CancellationToken cancellationToken = default);
}
