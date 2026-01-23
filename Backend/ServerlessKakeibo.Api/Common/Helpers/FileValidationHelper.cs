namespace ServerlessKakeibo.Api.Common.Helpers;

/// <summary>
/// ファイル検証ヘルパー
/// </summary>
public static class FileValidationHelper
{
    /// <summary>
    /// 画像ファイルの最大サイズ（バイト）
    /// </summary>
    public const long MaxImageFileSize = 10 * 1024 * 1024; // 10MB

    /// <summary>
    /// 許可される画像ファイル拡張子
    /// </summary>
    public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".pdf" };

    /// <summary>
    /// 許可されるMIMEタイプ
    /// </summary>
    public static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "application/pdf" };

    /// <summary>
    /// レシート画像ファイルの妥当性を検証
    /// </summary>
    /// <param name="file">検証対象のファイル</param>
    /// <exception cref="InvalidOperationException">検証エラー</exception>
    public static void ValidateReceiptImageFile(IFormFile file)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        if (file.Length == 0)
            throw new InvalidOperationException("空のファイルはアップロードできません");

        if (file.Length > MaxImageFileSize)
            throw new InvalidOperationException(
                $"ファイルサイズが上限({MaxImageFileSize / 1024 / 1024}MB)を超えています");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedImageExtensions.Contains(extension))
            throw new InvalidOperationException(
                $"サポートされていないファイル形式です。許可: {string.Join(", ", AllowedImageExtensions)}");

        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new InvalidOperationException(
                $"サポートされていないMIMEタイプです: {file.ContentType}");
    }

    /// <summary>
    /// ファイル名をサニタイズ（セキュリティ対策）
    /// </summary>
    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Guid.NewGuid().ToString();

        // 危険な文字を削除
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars));

        // パストラバーサル対策
        sanitized = sanitized.Replace("..", "").Replace("/", "").Replace("\\", "");

        return string.IsNullOrWhiteSpace(sanitized) ? Guid.NewGuid().ToString() : sanitized;
    }
}
