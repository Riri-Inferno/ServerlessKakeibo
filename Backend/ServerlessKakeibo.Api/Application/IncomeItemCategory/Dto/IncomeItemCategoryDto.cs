namespace ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;

/// <summary>
/// 給与項目カテゴリDTO
/// </summary>
public class IncomeItemCategoryDto
{
    /// <summary>
    /// カテゴリID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 項目名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 項目コード（LLM判定用）
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
    /// ユーザー追加のカスタム項目か
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
