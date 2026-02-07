namespace ServerlessKakeibo.Api.Application.TransactionCategory.Dto;

/// <summary>
/// 取引カテゴリ一覧結果
/// </summary>
public class TransactionCategoryListResult
{
    /// <summary>
    /// カテゴリ一覧
    /// </summary>
    public List<TransactionCategoryDto> Categories { get; set; } = new();

    /// <summary>
    /// 総件数
    /// </summary>
    public int TotalCount { get; set; }
}
