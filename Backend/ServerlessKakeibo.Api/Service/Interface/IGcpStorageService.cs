using ServerlessKakeibo.Api.Service.Models;

namespace ServerlessKakeibo.Api.Service.Interface;

/// <summary>
/// Google Cloud Storage サービスインターフェース
/// </summary>
public interface IGcpStorageService
{
    /// <summary>
    /// ファイルをGCSにアップロードする
    /// </summary>
    /// <param name="fileStream">アップロードするファイルのストリーム</param>
    /// <param name="destinationPath">バケット内の保存先パス</param>
    /// <param name="contentType">ファイルのMIMEタイプ</param>
    /// <param name="metadata">カスタムメタデータ（オプション）</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>アップロード結果</returns>
    Task<GcsUploadResult> UploadFileAsync(
        Stream fileStream,
        string destinationPath,
        string contentType,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ファイルをGCSから削除する
    /// </summary>
    /// <param name="objectPath">削除対象のオブジェクトパス</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>削除成功の場合true、ファイルが存在しない場合false</returns>
    Task<bool> DeleteFileAsync(
        string objectPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 複数ファイルを一括削除する
    /// </summary>
    /// <param name="objectPaths">削除対象のオブジェクトパスリスト</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>バッチ削除結果</returns>
    Task<GcsBatchDeleteResult> DeleteFilesAsync(
        IEnumerable<string> objectPaths,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// ファイルの存在を確認する
    /// </summary>
    /// <param name="objectPath">確認対象のオブジェクトパス</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>ファイルが存在する場合true</returns>
    Task<bool> FileExistsAsync(
        string objectPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 署名付き一時URLを生成する（プライベートバケット用）
    /// </summary>
    /// <param name="objectPath">対象のオブジェクトパス</param>
    /// <param name="expiration">有効期限</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>署名付きURL</returns>
    Task<string> GenerateSignedUrlAsync(
        string objectPath,
        TimeSpan expiration,
        CancellationToken cancellationToken = default);
}
