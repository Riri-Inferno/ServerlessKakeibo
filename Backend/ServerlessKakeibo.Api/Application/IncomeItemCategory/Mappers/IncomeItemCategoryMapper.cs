using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Application.IncomeItemCategory.Mappers;

/// <summary>
/// 給与項目カテゴリマッパー
/// </summary>
public static class IncomeItemCategoryMapper
{
    public static IncomeItemCategoryDto ToDto(UserIncomeItemCategoryEntity entity)
    {
        return new IncomeItemCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            ColorCode = entity.ColorCode,
            DisplayOrder = entity.DisplayOrder,
            IsCustom = entity.IsCustom,
            IsHidden = entity.IsHidden,
            MasterCategoryId = entity.MasterCategoryId
        };
    }

    public static List<IncomeItemCategoryDto> ToDtoList(List<UserIncomeItemCategoryEntity> entities)
    {
        return entities.Select(ToDto).ToList();
    }
}
