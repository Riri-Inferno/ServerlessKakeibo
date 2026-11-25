using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using ServerlessKakeibo.Api.Infrastructure.Data;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace ServerlessKakeibo.Api.Service.Authentication.OAuth;

/// <summary>
/// OAuth 認可サーバー（OpenIddict）の登録。
///
/// 既存のログイン（JWT）と API キーの認証には影響しない:
/// - 認証の既定のスキーム（JwtBearer）は変えない。OpenIddict は自分のエンドポイント（/oauth/*、/.well-known/*）だけを処理する
/// - OpenIddict.Validation（OpenIddict のトークンで API を認証する機能）は登録しない
/// </summary>
public static class OAuthServerServiceCollectionExtensions
{
    public const string AuthorizationEndpoint = "oauth/authorize";
    public const string TokenEndpoint = "oauth/token";

    public static IServiceCollection AddSeleneOAuthServer(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var section = configuration.GetSection(OAuthServerSettings.SectionName);
        services.Configure<OAuthServerSettings>(section);
        var settings = section.Get<OAuthServerSettings>() ?? new OAuthServerSettings();

        // テーブル（アプリケーション、承認、トークン）は、OAuth が無効でも作っておく
        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>();
                options.ReplaceApplicationManager(typeof(ClientIdMetadataDocumentApplicationManager<>));
            });

        services.AddHttpClient(ClientIdMetadataDocumentClient.HttpClientName, client =>
            {
                // Claude はメタデータ・トークンのエンドポイントを 10 秒で打ち切るので、その中に収める
                client.Timeout = TimeSpan.FromSeconds(5);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("selene-oauth/1.0");
            })
            .ConfigurePrimaryHttpMessageHandler(ClientIdMetadataDocumentClient.CreateHandler);
        services.AddSingleton<ClientIdMetadataDocumentClient>();
        services.AddSingleton<ConsentTicketService>();
        services.AddSingleton<DelegatedTokenService>();

        if (!settings.IsEnabled)
        {
            return services;
        }

        services.AddOpenIddict()
            .AddServer(options =>
            {
                options.SetIssuer(new Uri(settings.Issuer!));
                options.SetAuthorizationEndpointUris(AuthorizationEndpoint)
                       .SetTokenEndpointUris(TokenEndpoint);

                options.AllowAuthorizationCodeFlow()
                       .AllowRefreshTokenFlow();
                options.RequireProofKeyForCodeExchange();
                // PKCE は S256 だけを受け付ける（plain は使わせない）
                options.Configure(o => o.CodeChallengeMethods.Remove(CodeChallengeMethods.Plain));

                options.RegisterScopes(OAuthScopes.Read, OAuthScopes.Write);
                // resource パラメータ（RFC 8707）は MCP サーバーの URL だけを受け付け、トークンの aud にする
                options.RegisterResources(settings.McpResource!);

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(settings.AccessTokenLifetimeMinutes))
                       .SetRefreshTokenLifetime(TimeSpan.FromDays(settings.RefreshTokenLifetimeDays));

                AddKeys(options, settings, environment);

                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       // TLS は Cloudflare で終端し、クラスタ内は HTTP で届く。公開される URL は Issuer から作るので https になる
                       .DisableTransportSecurityRequirement();

                // メタデータを MCP 向けに整える
                // - CIMD 対応を載せる。Claude は client_id_metadata_document_supported と
                //   token_endpoint_auth_methods_supported の "none" がそろっていれば、CIMD を使う
                // - scopes_supported から offline_access（MCP の仕様で載せないほうがよい）と openid（ID トークンは発行しない）を外す
                options.AddEventHandler<OpenIddictServerEvents.HandleConfigurationRequestContext>(builder =>
                    builder.UseInlineHandler(context =>
                    {
                        context.Metadata["client_id_metadata_document_supported"] = true;
                        context.TokenEndpointAuthenticationMethods.Add(ClientAuthenticationMethods.None);
                        context.Scopes.Remove(Scopes.OfflineAccess);
                        context.Scopes.Remove(Scopes.OpenId);
                        return default;
                    })
                    // OpenIddict の組み込みのハンドラ（int.MaxValue - 100_000 から順に並ぶ）より後に動かす
                    .SetOrder(int.MaxValue));
            })
            // トークン交換で、MCP サーバーから受け取ったアクセストークンを検証する（OAuthTokenExchangeController）。
            // ASP.NET Core の認証スキームとしては登録しない（既存の API は OpenIddict のトークンを受け付けない）
            .AddValidation(options =>
            {
                options.UseLocalServer();
                // 宛先（aud）が MCP サーバーのトークンだけを受け付ける
                options.AddAudiences(settings.McpResource!);
                // 連携やトークンが失効していれば拒否する（連携の解除をすぐに反映する）
                options.EnableAuthorizationEntryValidation();
                options.EnableTokenEntryValidation();
            });

        return services;
    }

    private static void AddKeys(OpenIddictServerBuilder options, OAuthServerSettings settings, IHostEnvironment environment)
    {
        if (!string.IsNullOrWhiteSpace(settings.SigningKey) && !string.IsNullOrWhiteSpace(settings.EncryptionKey))
        {
            var rsa = RSA.Create();
            // .env ファイルでは改行を書けないので、"\n" と書かれた PEM も受け付ける
            rsa.ImportFromPem(settings.SigningKey.Replace("\\n", "\n"));
            options.AddSigningKey(new RsaSecurityKey(rsa));
            options.AddEncryptionKey(new SymmetricSecurityKey(Convert.FromBase64String(settings.EncryptionKey)));
            return;
        }

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "OAuth を有効にするには OAuth__SigningKey（RSA 秘密鍵の PEM）と OAuth__EncryptionKey（256 bit の Base64）が必要です");
        }

        // 開発用: 起動するたびに鍵が変わるので、発行済みのトークンは再起動で使えなくなる
        options.AddEphemeralSigningKey()
               .AddEphemeralEncryptionKey();
    }
}
