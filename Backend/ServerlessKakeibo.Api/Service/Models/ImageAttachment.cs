namespace ServerlessKakeibo.Api.Service.Models;

/// <summary>
/// 画像添付ファイル
/// </summary>
public class ImageAttachment
{
    /// <summary>
    /// Base64エンコードされた画像データ
    /// </summary>
    public string Base64Data { get; set; } = string.Empty;

    /// <summary>
    /// MIMEタイプ (image/jpeg, image/png, image/webp, image/gif)
    /// </summary>
    public string MimeType { get; set; } = "image/jpeg";
}
