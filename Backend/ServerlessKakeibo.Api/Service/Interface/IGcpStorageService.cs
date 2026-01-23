namespace ServerlessKakeibo.Api.Service.Interface;

public interface IGcpStorageService
{
    /// <summary>
    /// ファイルをアップロードする
    /// </summary>
    /// <param name="fileStream">ファイルストリーム</param>
    /// <param name="destinationPath">保存先パス（例: receipts/2026/image.jpg）</param>
    /// <param name="contentType">MIMEタイプ</param>
    /// <returns>アップロードされたオブジェクトの完全なパスまたはURL</returns>
    Task<string> UploadFileAsync(Stream fileStream, string destinationPath, string contentType);

    /// <summary>
    /// ファイルを削除する
    /// </summary>
    /// <param name="objectPath">オブジェクトのパス</param>
    Task DeleteFileAsync(string objectPath);
}
