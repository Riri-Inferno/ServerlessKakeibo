using System.Text.Json.Serialization;

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
    [JsonConverter(typeof(JsonStringEnumConverter))]
    Success = 0,

    /// <summary>
    /// 失敗：リクエストが不正
    /// 400 Bad Request
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    InvalidRequest,

    /// <summary>
    /// 失敗：認証エラー（将来利用予定）
    /// 401 Unauthorized
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    Unauthorized,

    /// <summary>
    /// 失敗: 権限エラー
    /// 403 Forbidden
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    Forbidden,

    /// <summary>
    /// 失敗：リソースが見つからないエラー
    /// 404 NotFound
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    NotFound,

    /// <summary>
    /// 失敗：パースエラー
    /// </summary>
    /// 422 ParseError
    [JsonConverter(typeof(JsonStringEnumConverter))]
    ParseFailed,

    /// <summary>
    /// 失敗：サポートされていないドキュメント
    /// 422 UnsupportedDocument
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    UnsupportedDocument,

    /// <summary>
    /// 失敗：内部エラー
    /// 500 Internal Server Error
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    InternalError
}
