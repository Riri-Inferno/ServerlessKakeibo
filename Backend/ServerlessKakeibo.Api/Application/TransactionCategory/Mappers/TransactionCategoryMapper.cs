using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Application.TransactionCategory.Mappers;

/// <summary>
/// 取引カテゴリマッパー
/// </summary>
public static class TransactionCategoryMapper
{
    /// <summary>
    /// エンティティをDTOに変換
    /// </summary>
    public static TransactionCategoryDto ToDto(UserTransactionCategoryEntity entity)
    {
        return new TransactionCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code,
            ColorCode = entity.ColorCode,
            DisplayOrder = entity.DisplayOrder,
            IsIncome = entity.IsIncome,
            IsCustom = entity.IsCustom,
            IsHidden = entity.IsHidden,
            MasterCategoryId = entity.MasterCategoryId
        };
    }

    /// <summary>
    /// エンティティリストをDTOリストに変換
    /// </summary>
    public static List<TransactionCategoryDto> ToDtoList(List<UserTransactionCategoryEntity> entities)
    {
        return entities.Select(ToDto).ToList();
    }
}
