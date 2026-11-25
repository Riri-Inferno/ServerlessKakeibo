using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ServerlessKakeibo.Api.Service.Authentication.OAuth;

/// <summary>
/// Client ID Metadata Document（CIMD）の取得と検証。
/// client_id が HTTPS の URL のとき、その URL からクライアントの情報（名前、コールバック URL など）を取得する。
/// 仕様: https://modelcontextprotocol.io/specification/latest/basic/authorization （Client ID Metadata Documents）
/// </summary>
public class ClientIdMetadataDocumentClient
{
    public const string HttpClientName = "oauth-cimd";

    private const int MaxDocumentBytes = 64 * 1024;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ClientIdMetadataDocumentClient> _logger;

    public ClientIdMetadataDocumentClient(
        IHttpClientFactory httpClientFactory,
        ILogger<ClientIdMetadataDocumentClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// CIMD として扱う client_id か（パス付きの HTTPS の URL）
    /// </summary>
    public static bool IsMetadataDocumentClientId(string clientId) =>
        Uri.TryCreate(clientId, UriKind.Absolute, out var uri) &&
        uri.Scheme == Uri.UriSchemeHttps &&
        string.IsNullOrEmpty(uri.Fragment) &&
        uri.AbsolutePath.Length > 1;

    /// <summary>
    /// CIMD を取得して検証する。取得できない、または検証に通らない場合は null
    /// </summary>
    public async Task<ClientIdMetadataDocument?> FetchAsync(string clientId, CancellationToken cancellationToken)
    {
        if (!IsMetadataDocumentClientId(clientId))
        {
            return null;
        }

        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.GetAsync(
                clientId, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("CIMD の取得に失敗しました: {ClientId} ({StatusCode})", clientId, (int)response.StatusCode);
                return null;
            }

            if (response.Content.Headers.ContentLength > MaxDocumentBytes)
            {
                _logger.LogWarning("CIMD が大きすぎます: {ClientId}", clientId);
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var buffer = new byte[MaxDocumentBytes + 1];
            var length = await stream.ReadAtLeastAsync(buffer, buffer.Length, throwOnEndOfStream: false, cancellationToken);
            if (length > MaxDocumentBytes)
            {
                _logger.LogWarning("CIMD が大きすぎます: {ClientId}", clientId);
                return null;
            }

            var document = JsonSerializer.Deserialize<ClientIdMetadataDocument>(buffer.AsSpan(0, length));
            return Validate(clientId, document) ? document : null;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "CIMD の取得または解析に失敗しました: {ClientId}", clientId);
            return null;
        }
    }

    private bool Validate(string clientId, ClientIdMetadataDocument? document)
    {
        if (document is null)
        {
            return false;
        }

        // 取得した文書の client_id は、取得に使った URL と完全に一致しなければならない（MUST）
        if (!string.Equals(document.ClientId, clientId, StringComparison.Ordinal))
        {
            _logger.LogWarning("CIMD の client_id が URL と一致しません: {ClientId}", clientId);
            return false;
        }

        if (document.RedirectUris is not { Count: > 0 } ||
            !document.RedirectUris.All(IsAllowedRedirectUri))
        {
            _logger.LogWarning("CIMD の redirect_uris が不正です: {ClientId}", clientId);
            return false;
        }

        // secret を持たない公開クライアントだけを受け付ける（PKCE で保護する）
        if (document.TokenEndpointAuthMethod is not (null or "none"))
        {
            _logger.LogWarning("CIMD の token_endpoint_auth_method に未対応です: {ClientId} ({Method})",
                clientId, document.TokenEndpointAuthMethod);
            return false;
        }

        return true;
    }

    /// <summary>
    /// コールバック URL は HTTPS か、ループバック（localhost / 127.0.0.1）の HTTP のみ（MUST）
    /// </summary>
    private static bool IsAllowedRedirectUri(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        string.IsNullOrEmpty(uri.Fragment) &&
        (uri.Scheme == Uri.UriSchemeHttps || (uri.Scheme == Uri.UriSchemeHttp && uri.IsLoopback));

    /// <summary>
    /// CIMD 取得用の HttpClient のハンドラ。
    /// 任意の URL を取りに行くため、接続先がプライベートなアドレスなら接続しない（SSRF 対策）。
    /// リダイレクトにも従わない
    /// </summary>
    public static SocketsHttpHandler CreateHandler() => new()
    {
        AllowAutoRedirect = false,
        ConnectTimeout = TimeSpan.FromSeconds(3),
        ConnectCallback = async (context, cancellationToken) =>
        {
            var addresses = await Dns.GetHostAddressesAsync(context.DnsEndPoint.Host, cancellationToken);
            var address = addresses.FirstOrDefault(IsPublicAddress)
                ?? throw new HttpRequestException($"公開されていないアドレスへの接続は許可されていません: {context.DnsEndPoint.Host}");

            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            try
            {
                await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port), cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
    };

    private static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (IPAddress.IsLoopback(address) || address.IsIPv6LinkLocal || address.IsIPv6SiteLocal ||
            address.IsIPv6UniqueLocal || address.IsIPv6Multicast ||
            address.Equals(IPAddress.Any) || address.Equals(IPAddress.IPv6Any))
        {
            return false;
        }

        if (address.AddressFamily == AddressFamily.InterNetwork)
        {
            var b = address.GetAddressBytes();
            return !(b[0] == 10 ||
                     b[0] == 0 ||
                     (b[0] == 100 && b[1] >= 64 && b[1] <= 127) ||   // CGNAT
                     (b[0] == 169 && b[1] == 254) ||                 // リンクローカル
                     (b[0] == 172 && b[1] >= 16 && b[1] <= 31) ||
                     (b[0] == 192 && b[1] == 168) ||
                     b[0] >= 224);                                    // マルチキャスト・予約
        }

        return true;
    }
}

/// <summary>
/// CIMD のうち、この認可サーバーが使う項目
/// </summary>
public class ClientIdMetadataDocument
{
    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }

    [JsonPropertyName("client_name")]
    public string? ClientName { get; set; }

    [JsonPropertyName("client_uri")]
    public string? ClientUri { get; set; }

    [JsonPropertyName("redirect_uris")]
    public List<string>? RedirectUris { get; set; }

    [JsonPropertyName("token_endpoint_auth_method")]
    public string? TokenEndpointAuthMethod { get; set; }
}
