namespace ServerlessKakeibo.Api.Service.Authentication.OAuth;

/// <summary>
/// OAuth 認可サーバー（Claude などの MCP クライアント向け）の設定。appsettings / 環境変数の "OAuth" セクション。
/// Issuer・McpResource・ConsentPageUrl がそろっていないときは、認可サーバーを有効にしない（既存の認証には影響しない）。
/// </summary>
public class OAuthServerSettings
{
    public const string SectionName = "OAuth";

    /// <summary>
    /// 認可サーバーの issuer。例: https://api-kakeibo.riri-inferno.com/
    /// メタデータ（/.well-known/oauth-authorization-server）の issuer と、各エンドポイントの URL の基準になる
    /// </summary>
    public string? Issuer { get; set; }

    /// <summary>
    /// アクセストークンの宛先（aud）になる MCP サーバーの URL。例: https://mcp-kakeibo.riri-inferno.com
    /// 認可リクエストの resource パラメータは、この値以外を受け付けない
    /// </summary>
    public string? McpResource { get; set; }

    /// <summary>
    /// ログインと承認を行う画面（フロントエンド）の URL。例: https://kakeibo.riri-inferno.com/oauth/consent
    /// 認可リクエストのパラメータを付けて、ここにリダイレクトする
    /// </summary>
    public string? ConsentPageUrl { get; set; }

    /// <summary>
    /// トークンの署名に使う RSA 秘密鍵（PEM）。OpenIddict は非対称鍵を必須とする。
    /// 未設定の場合、Development では起動ごとに作る一時的な鍵を使い、それ以外では起動に失敗する
    /// </summary>
    public string? SigningKey { get; set; }

    /// <summary>
    /// 認可コード・リフレッシュトークンなどの暗号化に使う共通鍵（256 bit を Base64 で）。
    /// 未設定時の扱いは SigningKey と同じ
    /// </summary>
    public string? EncryptionKey { get; set; }

    /// <summary>アクセストークンの有効期間（分）</summary>
    public int AccessTokenLifetimeMinutes { get; set; } = 60;

    /// <summary>リフレッシュトークンの有効期間（日）。使うたびに新しいものに入れ替わる</summary>
    public int RefreshTokenLifetimeDays { get; set; } = 30;

    public bool IsEnabled =>
        !string.IsNullOrWhiteSpace(Issuer) &&
        !string.IsNullOrWhiteSpace(McpResource) &&
        !string.IsNullOrWhiteSpace(ConsentPageUrl);
}

/// <summary>
/// OAuth で扱うスコープ。API キーと同じく read / write で、write だけの許可はしない
/// </summary>
public static class OAuthScopes
{
    public const string Read = "read";
    public const string Write = "write";

    public static readonly IReadOnlyList<string> All = [Read, Write];
}
