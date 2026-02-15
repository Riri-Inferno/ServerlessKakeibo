namespace ServerlessKakeibo.Api.Application.TransactionQuery.Dto;

/// <summary>
/// ユーザー取引カテゴリDTO
/// </summary>
public class UserTransactionCategoryDto
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
    /// 表示色
    /// </summary>
    public string ColorCode { get; set; } = string.Empty;

    /// <summary>
    /// 収入カテゴリか
    /// </summary>
    public bool IsIncome { get; set; }

    /// <summary>
    /// カスタムカテゴリか
    /// </summary>
    public bool IsCustom { get; set; }
}
