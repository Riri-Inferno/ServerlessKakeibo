using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// レシート画像添付リクエスト
/// </summary>
public class AttachReceiptRequest
{
    /// <summary>
    /// アップロードするレシート画像ファイル
    /// </summary>
    [Required(ErrorMessage = "ファイルは必須です")]
    public IFormFile File { get; set; } = null!;
}
