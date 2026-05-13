namespace ServerlessKakeibo.Api.Application.ApiKey.Dto;

/// <summary>
/// APIキー発行結果
/// 平文キー (Key) は **このレスポンスでのみ** 返却される
/// </summary>
public record CreateApiKeyResult(
    Guid Id,
    string Name,
    string Key,
    string KeyPrefix,
    IReadOnlyList<string> Scopes,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset CreatedAt
);
