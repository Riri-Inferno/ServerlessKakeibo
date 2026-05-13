using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Service.Interface;

namespace ServerlessKakeibo.Api.Service.Authentication.ApiKey;

/// <summary>
/// APIキー認証ハンドラ
/// Authorization: Bearer slk_... ヘッダを受け取り、DB に保存されたハッシュと照合して認証する。
/// "slk_" で始まらないトークンは JWT 用ハンドラに譲る。
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHashService _passwordHashService;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ApplicationDbContext dbContext,
        IPasswordHashService passwordHashService)
        : base(options, loggerFactory, encoder)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _passwordHashService = passwordHashService ?? throw new ArgumentNullException(nameof(passwordHashService));
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Authorization ヘッダが無ければ自分の出番ではない
        if (!Request.Headers.TryGetValue("Authorization", out var rawHeader))
        {
            return AuthenticateResult.NoResult();
        }

        if (!AuthenticationHeaderValue.TryParse(rawHeader, out var headerValue)
            || !string.Equals(headerValue.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(headerValue.Parameter))
        {
            return AuthenticateResult.NoResult();
        }

        var token = headerValue.Parameter;

        // APIキーの接頭辞でなければ JWT ハンドラに譲る
        if (!token.StartsWith(ApiKeyAuthenticationDefaults.KeyPrefix, StringComparison.Ordinal))
        {
            return AuthenticateResult.NoResult();
        }

        // 形式チェック（プレフィックス分の長さは最低限必要）
        if (token.Length < ApiKeyAuthenticationDefaults.LookupPrefixLength)
        {
            return AuthenticateResult.Fail("APIキーの形式が不正です");
        }

        var prefix = token.Substring(0, ApiKeyAuthenticationDefaults.LookupPrefixLength);
        var now = DateTimeOffset.UtcNow;

        // プレフィックスで候補を絞り込む（失効・期限切れも除外）
        var candidates = await _dbContext.ApiKeys
            .Where(ak => ak.KeyPrefix == prefix
                       && ak.RevokedAt == null
                       && (ak.ExpiresAt == null || ak.ExpiresAt > now))
            .ToListAsync();

        ApiKeyEntity? matched = null;
        foreach (var candidate in candidates)
        {
            if (_passwordHashService.VerifyPassword(token, candidate.KeyHash))
            {
                matched = candidate;
                break;
            }
        }

        if (matched == null)
        {
            Logger.LogWarning("APIキー認証に失敗しました。Prefix: {Prefix}", prefix);
            return AuthenticateResult.Fail("APIキーが無効です");
        }

        // LastUsedAt を直接 UPDATE（xmin に影響しない、SaveChanges の自動更新フックも回避）
        try
        {
            await _dbContext.ApiKeys
                .Where(ak => ak.Id == matched.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(ak => ak.LastUsedAt, now));
        }
        catch (Exception ex)
        {
            // LastUsedAt の更新失敗は認証可否に影響させない
            Logger.LogWarning(ex, "LastUsedAt の更新に失敗しました。ApiKeyId: {ApiKeyId}", matched.Id);
        }

        // 既存の ClaimsPrincipalExtensions が JWT と同じキーで読めるようにする
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, matched.UserId.ToString()),
            new Claim("tenant_id", matched.TenantId.ToString()),
            new Claim("api_key_id", matched.Id.ToString()),
        };

        foreach (var scope in matched.Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            claims.Add(new Claim("scope", scope));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}
