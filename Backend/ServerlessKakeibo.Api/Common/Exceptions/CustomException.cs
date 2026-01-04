using System.Net;

namespace ServerlessKakeibo.Api.Common.Exceptions;

/// <summary>
/// カスタム例外クラス
/// </summary>
public class CustomException : Exception
{
    public ExceptionType ExceptionType { get; }

    public CustomException(ExceptionType exceptionType)
        : base(exceptionType.Message)
    {
        ExceptionType = exceptionType;
    }

    public CustomException(ExceptionType exceptionType, Exception innerException)
        : base(exceptionType.Message, innerException)
    {
        ExceptionType = exceptionType;
    }
}

/// <summary>
/// 例外タイプ
/// </summary>
public class ExceptionType
{
    public HttpStatusCode StatusCode { get; }
    public string Message { get; }

    public ExceptionType(HttpStatusCode statusCode, string message)
    {
        StatusCode = statusCode;
        Message = message;
    }
}

/// <summary>
/// エラーメッセージ定義
/// </summary>
public static class ErrorMessages
{
    /// <summary>
    /// 認証系
    /// </summary>
    public static class Auth
    {
        public const string MissingServiceAccountJson = "サービスアカウントJSONが設定されていません";
        public const string AccessTokenFailed = "アクセストークンの取得に失敗しました";
        public const string AccessTokenInvalid = "アクセストークンが無効です";
    }

    /// <summary>
    /// VertexAI
    /// </summary>
    public static class VertexAi
    {
        public const string ApiCallFailed = "Vertex AI API呼び出しが失敗しました";
        public const string RequestTimeout = "Vertex AI APIリクエストがタイムアウトしました";
        public const string EmptyResponse = "Vertex AIからの応答が空です";
        public const string ResponseParseFailed = "Vertex AIレスポンスの解析に失敗しました";
        public const string MaxRetriesReached = "最大リトライ回数に到達しました";
        public const string RateLimitExceeded = "レート制限に到達しました";
    }
}
