namespace ServerlessKakeibo.Api.Service.Authentication.ApiKey;

/// <summary>
/// APIキー認証スキームに関する定数
/// </summary>
public static class ApiKeyAuthenticationDefaults
{
    /// <summary>
    /// 認証スキーム名
    /// </summary>
    public const string AuthenticationScheme = "ApiKey";

    /// <summary>
    /// キーの接頭辞（"SELENE Long-lived Key"）
    /// この接頭辞で JWT と区別する
    /// </summary>
    public const string KeyPrefix = "slk_";

    /// <summary>
    /// 候補検索に使用するプレフィックス長（"slk_" + 8文字 = 12文字）
    /// </summary>
    public const int LookupPrefixLength = 12;

    /// <summary>
    /// JWT と APIキーの両方を受け付けるエンドポイントで使う AuthenticationSchemes
    /// </summary>
    public const string JwtOrApiKeySchemes = "Bearer," + AuthenticationScheme;

    /// <summary>
    /// 読み取りスコープを要求する認可ポリシー名
    /// </summary>
    public const string ReadPolicy = "ApiKey.Read";

    /// <summary>
    /// 書き込みスコープを要求する認可ポリシー名
    /// </summary>
    public const string WritePolicy = "ApiKey.Write";
}
