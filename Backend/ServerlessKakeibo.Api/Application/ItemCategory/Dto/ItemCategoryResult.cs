namespace ServerlessKakeibo.Api.Application.ItemCategory.Dto;

/// <summary>
/// 商品カテゴリ操作結果
/// </summary>
public class ItemCategoryResult
{
    /// <summary>
    /// カテゴリ情報
    /// </summary>
    public ItemCategoryDto Category { get; set; } = default!;

    /// <summary>
    /// 処理結果メッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
