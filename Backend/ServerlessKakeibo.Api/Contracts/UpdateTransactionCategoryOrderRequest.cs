using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// 取引カテゴリ並び順一括更新リクエスト
/// </summary>
public class UpdateTransactionCategoryOrderRequest
{
    [Required(ErrorMessage = "並び順情報は必須です")]
    [MinLength(1, ErrorMessage = "最低1件以上のカテゴリが必要です")]
    public List<CategoryOrderItem> Orders { get; set; } = new();
}

/// <summary>
/// カテゴリ並び順情報
/// </summary>
public class CategoryOrderItem
{
    [Required(ErrorMessage = "カテゴリIDは必須です")]
    public Guid Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "表示順序は1以上の値を指定してください")]
    public int DisplayOrder { get; set; }
}
