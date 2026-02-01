namespace ServerlessKakeibo.Api.Service.Models;

/// <summary>
/// GitHubユーザー情報
/// </summary>
public record GitHubUserInfo
{
    /// <summary>
    /// GitHubのユーザーID（数値）
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// GitHubのログイン名
    /// </summary>
    public string Login { get; init; } = string.Empty;

    /// <summary>
    /// 表示名
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// メールアドレス
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// アバター画像URL
    /// </summary>
    public string? AvatarUrl { get; init; }
}
