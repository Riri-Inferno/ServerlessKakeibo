namespace ServerlessKakeibo.Api.Contracts.Enums;

/// <summary>
/// API ステータス
/// </summary>
public enum ApiStatus
{
    /// <summary>
    /// 成功
    /// 200 OK
    /// </summary>
    Success = 0,

    /// <summary>
    /// 失敗：リクエストが不正
    /// 400 Bad Request
    /// </summary>
    InvalidRequest,

    /// <summary>
    /// 失敗：認証エラー（将来利用予定）
    /// 401 Unauthorized
    /// </summary>
    Unauthorized,

    /// <summary>
    /// 失敗：パースエラー
    /// </summary>
    /// 422 ParseError
    ParseFailed,

    /// <summary>
    /// 失敗：サポートされていないドキュメント
    /// 422 UnsupportedDocument
    /// </summary>
    UnsupportedDocument,

    /// <summary>
    /// 失敗：内部エラー
    /// 500 Internal Server Error
    /// </summary>
    InternalError
}
