namespace ServerlessKakeibo.Api.Application.TransactionCategory.Dto;

/// <summary>
/// 取引カテゴリDTO
/// </summary>
public class TransactionCategoryDto
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
    /// カテゴリコード（LLM判定用）
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 表示色（HEX形式）
    /// </summary>
    public string ColorCode { get; set; } = string.Empty;

    /// <summary>
    /// 表示順序
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// 収入カテゴリか
    /// </summary>
    public bool IsIncome { get; set; }

    /// <summary>
    /// ユーザー追加のカスタムカテゴリか
    /// </summary>
    public bool IsCustom { get; set; }

    /// <summary>
    /// 非表示フラグ
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// 元マスタカテゴリID（カスタムの場合null）
    /// </summary>
    public Guid? MasterCategoryId { get; set; }
}
