using ServerlessKakeibo.Api.Application.ItemCategory.Dto;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Application.ItemCategory.Mappers;

/// <summary>
/// 商品カテゴリマッパー
/// </summary>
public static class ItemCategoryMapper
{
    public static ItemCategoryDto ToDto(UserItemCategoryEntity entity)
    {
        return new ItemCategoryDto
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

    public static List<ItemCategoryDto> ToDtoList(List<UserItemCategoryEntity> entities)
    {
        return entities.Select(ToDto).ToList();
    }
}
