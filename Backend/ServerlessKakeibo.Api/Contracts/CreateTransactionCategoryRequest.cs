using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// 取引カテゴリ作成リクエスト
/// </summary>
public class CreateTransactionCategoryRequest
{
    [Required(ErrorMessage = "カテゴリ名は必須です")]
    [MaxLength(100, ErrorMessage = "カテゴリ名は100文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "表示色は必須です")]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "表示色は#FFFFFFの形式で入力してください")]
    public string ColorCode { get; set; } = string.Empty;

    public bool IsIncome { get; set; }
}
