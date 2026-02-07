namespace ServerlessKakeibo.Api.Application.ItemCategory.Dto;

/// <summary>
/// 商品カテゴリ一覧結果
/// </summary>
public class ItemCategoryListResult
{
    /// <summary>
    /// カテゴリ一覧
    /// </summary>
    public List<ItemCategoryDto> Categories { get; set; } = new();

    /// <summary>
    /// 総件数
    /// </summary>
    public int TotalCount { get; set; }
}
