using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.IncomeItemCategory.Usecases;

/// <summary>
/// 給与項目カテゴリ並び順一括更新ユースケース
/// </summary>
public interface IUpdateIncomeItemCategoryOrderUseCase
{
    Task<IncomeItemCategoryListResult> ExecuteAsync(
        Guid userId,
        UpdateIncomeItemCategoryOrderRequest request,
        CancellationToken cancellationToken = default);
}
