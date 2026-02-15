namespace ServerlessKakeibo.Api.Application.TransactionQuery.Dto;

/// <summary>
/// ユーザー収入項目カテゴリDTO
/// </summary>
public class UserIncomeItemCategoryDto
{
    /// <summary>
    /// カテゴリID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// カテゴリ名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// カラーコード
    /// </summary>
    public string ColorCode { get; set; } = string.Empty;

    /// <summary>
    /// カスタムカテゴリフラグ
    /// </summary>
    public bool IsCustom { get; set; }
}
