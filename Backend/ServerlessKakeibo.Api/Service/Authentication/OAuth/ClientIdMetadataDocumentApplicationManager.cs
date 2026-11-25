using System.Text.Json;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ServerlessKakeibo.Api.Service.Authentication.OAuth;

/// <summary>
/// URL 形式の client_id（CIMD）に対応した ApplicationManager。
/// OpenIddict は事前に登録されたクライアントしか受け付けないため、未登録の client_id が CIMD の URL なら、
/// その場で文書を取得・検証してクライアントとして登録する。一定時間たったら取得し直して内容を更新する。
/// </summary>
public class ClientIdMetadataDocumentApplicationManager<TApplication> : OpenIddictApplicationManager<TApplication>
    where TApplication : class
{
    /// <summary>CIMD から登録したクライアントの印と、取得した日時（Properties に保存する）</summary>
    private const string FetchedAtProperty = "cimd_fetched_at";

    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(24);

    private readonly ClientIdMetadataDocumentClient _documentClient;
    private readonly OAuthServerSettings _settings;

    public ClientIdMetadataDocumentApplicationManager(
        IOpenIddictApplicationCache<TApplication> cache,
        ILogger<OpenIddictApplicationManager<TApplication>> logger,
        IOptionsMonitor<OpenIddictCoreOptions> options,
        IOpenIddictApplicationStore<TApplication> store,
        ClientIdMetadataDocumentClient documentClient,
        IOptions<OAuthServerSettings> settings)
        : base(cache, logger, options, store)
    {
        _documentClient = documentClient;
        _settings = settings.Value;
    }

    public override async ValueTask<TApplication?> FindByClientIdAsync(
        string identifier, CancellationToken cancellationToken = default)
    {
        var application = await base.FindByClientIdAsync(identifier, cancellationToken);

        if (!ClientIdMetadataDocumentClient.IsMetadataDocumentClientId(identifier))
        {
            return application;
        }

        if (application is not null && !await IsStaleAsync(application, cancellationToken))
        {
            return application;
        }

        var document = await _documentClient.FetchAsync(identifier, cancellationToken);
        if (document is null)
        {
            // 取得し直しに失敗した場合は、前回取得した内容をそのまま使う
            return application;
        }

        var descriptor = CreateDescriptor(identifier, document, _settings.McpResource);

        if (application is null)
        {
            return await CreateAsync(descriptor, cancellationToken);
        }

        await UpdateAsync(application, descriptor, cancellationToken);
        return application;
    }

    private async ValueTask<bool> IsStaleAsync(TApplication application, CancellationToken cancellationToken)
    {
        var properties = await Store.GetPropertiesAsync(application, cancellationToken);
        if (!properties.TryGetValue(FetchedAtProperty, out var value) ||
            value.ValueKind != JsonValueKind.String ||
            !DateTimeOffset.TryParse(value.GetString(), out var fetchedAt))
        {
            // CIMD 以外の方法で登録されたクライアントは取得し直さない
            return false;
        }

        // MCP サーバーの URL（resource）の設定が変わった場合も、許可を付け直すために取得し直す
        if (!string.IsNullOrEmpty(_settings.McpResource) &&
            !await HasPermissionAsync(application, Permissions.Prefixes.Resource + _settings.McpResource, cancellationToken))
        {
            return true;
        }

        return DateTimeOffset.UtcNow - fetchedAt > RefreshInterval;
    }

    private static OpenIddictApplicationDescriptor CreateDescriptor(
        string clientId, ClientIdMetadataDocument document, string? mcpResource)
    {
        var redirectUris = document.RedirectUris!.Select(uri => new Uri(uri)).ToList();

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            ClientType = ClientTypes.Public,
            // ループバックのコールバックだけのクライアント（Claude Code など）は native として登録する。
            // OpenIddict は native のときだけ、ループバックのコールバックのポート違いを許す（RFC 8252）
            ApplicationType = redirectUris.All(uri => uri.IsLoopback) ? ApplicationTypes.Native : ApplicationTypes.Web,
            ConsentType = ConsentTypes.Explicit,
            DisplayName = string.IsNullOrWhiteSpace(document.ClientName) ? new Uri(clientId).Host : document.ClientName,
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Prefixes.Scope + OAuthScopes.Read,
                Permissions.Prefixes.Scope + OAuthScopes.Write,
                // メタデータには載せていないが、クライアントが求めてきた場合は受け付ける
                Permissions.Prefixes.Scope + Scopes.OfflineAccess,
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange,
            },
        };

        // MCP サーバー宛てのトークンだけを発行できるようにする（resource パラメータの許可）
        if (!string.IsNullOrEmpty(mcpResource))
        {
            descriptor.Permissions.Add(Permissions.Prefixes.Resource + mcpResource);
        }

        foreach (var uri in redirectUris)
        {
            descriptor.RedirectUris.Add(uri);
        }

        descriptor.Properties[FetchedAtProperty] = JsonSerializer.SerializeToElement(DateTimeOffset.UtcNow.ToString("O"));
        if (!string.IsNullOrWhiteSpace(document.ClientUri))
        {
            descriptor.Properties["client_uri"] = JsonSerializer.SerializeToElement(document.ClientUri);
        }

        return descriptor;
    }
}
