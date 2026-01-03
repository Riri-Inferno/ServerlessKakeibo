using Microsoft.AspNetCore.Http;

namespace ServerlessKakeibo.Api.Common.Helpers;

/// <summary>
/// 画像処理に関するヘルパークラス
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// サポートされている画像MIMEタイプ
    /// </summary>
    private static readonly HashSet<string> SupportedImageMimeTypes = new()
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/bmp"
    };

    /// <summary>
    /// 画像ファイルの拡張子とMIMEタイプのマッピング
    /// </summary>
    private static readonly Dictionary<string, string> ExtensionToMimeType = new()
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".webp", "image/webp" },
        { ".bmp", "image/bmp" }
    };

    /// <summary>
    /// IFormFileをBase64文字列に変換
    /// </summary>
    public static async Task<string> ConvertToBase64Async(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("ファイルが空です", nameof(file));

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        byte[] fileBytes = memoryStream.ToArray();
        return Convert.ToBase64String(fileBytes);
    }

    /// <summary>
    /// ファイルが画像ファイルかどうかを検証（MIMEタイプとマジックナンバーで判定）
    /// </summary>
    public static async Task<bool> CheckIfImageFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        // MIMEタイプチェック
        if (!IsImageMimeType(file.ContentType))
            return false;

        // マジックナンバーによる実際のファイル形式チェック
        return await CheckImageMagicNumberAsync(file);
    }

    /// <summary>
    /// MIMEタイプが画像かどうか判定
    /// </summary>
    public static bool IsImageMimeType(string? mimeType)
    {
        return !string.IsNullOrWhiteSpace(mimeType) &&
               SupportedImageMimeTypes.Contains(mimeType.ToLowerInvariant());
    }

    /// <summary>
    /// ファイル拡張子から実際のMIMEタイプを取得
    /// </summary>
    public static string? GetMimeTypeFromFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        return extension != null && ExtensionToMimeType.TryGetValue(extension, out var mimeType)
            ? mimeType
            : null;
    }

    /// <summary>
    /// マジックナンバーで画像ファイルの形式をチェック
    /// </summary>
    private static async Task<bool> CheckImageMagicNumberAsync(IFormFile file)
    {
        const int headerBytesToRead = 16;
        byte[] headerBytes = new byte[headerBytesToRead];

        using var stream = file.OpenReadStream();
        var bytesToRead = Math.Min(headerBytesToRead, (int)file.Length);
        await stream.ReadAsync(headerBytes, 0, bytesToRead);

        // JPEG
        if (headerBytes[0] == 0xFF && headerBytes[1] == 0xD8)
            return true;

        // PNG
        if (headerBytes[0] == 0x89 && headerBytes[1] == 0x50 &&
            headerBytes[2] == 0x4E && headerBytes[3] == 0x47)
            return true;

        // GIF
        if (headerBytes[0] == 0x47 && headerBytes[1] == 0x49 && headerBytes[2] == 0x46)
            return true;

        // BMP
        if (headerBytes[0] == 0x42 && headerBytes[1] == 0x4D)
            return true;

        // WebP (ファイルサイズが12バイト以上の場合のみチェック)
        if (file.Length >= 12 &&
            headerBytes[0] == 0x52 && headerBytes[1] == 0x49 &&
            headerBytes[2] == 0x46 && headerBytes[3] == 0x46 &&
            headerBytes[8] == 0x57 && headerBytes[9] == 0x45 &&
            headerBytes[10] == 0x42 && headerBytes[11] == 0x50)
            return true;

        return false;
    }
}
