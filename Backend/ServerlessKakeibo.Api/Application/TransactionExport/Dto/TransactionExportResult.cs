namespace ServerlessKakeibo.Api.Application.TransactionExport.Dto;

/// <summary>
/// エクスポート結果
/// </summary>
public class TransactionExportResult
{
    /// <summary>
    /// エクスポートされた取引件数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 添付画像を含めた件数
    /// </summary>
    public int ImagesIncludedCount { get; set; }

    /// <summary>
    /// 画像取得に失敗した件数
    /// </summary>
    public int ImagesFailedCount { get; set; }

    /// <summary>
    /// 警告メッセージ
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// ZIPファイルバイナリ（Base64エンコード済み）
    /// </summary>
    public string ZipDataBase64 { get; set; } = string.Empty;

    /// <summary>
    /// ファイル名
    /// </summary>
    public string FileName { get; set; } = string.Empty;
}
