namespace ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;

/// <summary>
/// 給与項目カテゴリ一覧結果
/// </summary>
public class IncomeItemCategoryListResult
{
    /// <summary>
    /// カテゴリ一覧
    /// </summary>
    public List<IncomeItemCategoryDto> Categories { get; set; } = new();

    /// <summary>
    /// 総件数
    /// </summary>
    public int TotalCount { get; set; }
}
