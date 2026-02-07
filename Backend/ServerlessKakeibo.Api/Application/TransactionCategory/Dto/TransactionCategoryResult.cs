namespace ServerlessKakeibo.Api.Application.TransactionCategory.Dto;

/// <summary>
/// 取引カテゴリ操作結果
/// </summary>
public class TransactionCategoryResult
{
    /// <summary>
    /// カテゴリ情報
    /// </summary>
    public TransactionCategoryDto Category { get; set; } = default!;

    /// <summary>
    /// 処理結果メッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
