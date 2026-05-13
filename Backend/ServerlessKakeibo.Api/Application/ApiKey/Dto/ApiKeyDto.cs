namespace ServerlessKakeibo.Api.Application.ApiKey.Dto;

/// <summary>
/// APIキー情報 DTO（一覧表示用）
/// 平文キーやハッシュは含まない
/// </summary>
public record ApiKeyDto(
    Guid Id,
    string Name,
    string KeyPrefix,
    IReadOnlyList<string> Scopes,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? LastUsedAt,
    DateTimeOffset? RevokedAt,
    DateTimeOffset CreatedAt
);
