using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// GitHub ログインリクエスト
/// </summary>
public record GitHubLoginRequest
{
    /// <summary>
    /// GitHub OAuth から受け取った認証コード
    /// </summary>
    [Required(ErrorMessage = "認証コードは必須です")]
    public string Code { get; init; } = string.Empty;

    /// <summary>
    /// CSRF対策用のstate（オプション）
    /// </summary>
    public string? State { get; init; }
}
