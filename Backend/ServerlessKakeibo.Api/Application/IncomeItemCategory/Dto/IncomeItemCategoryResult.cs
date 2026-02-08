namespace ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;

/// <summary>
/// 給与項目カテゴリ操作結果
/// </summary>
public class IncomeItemCategoryResult
{
    /// <summary>
    /// カテゴリ情報
    /// </summary>
    public IncomeItemCategoryDto Category { get; set; } = default!;

    /// <summary>
    /// 処理結果メッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
