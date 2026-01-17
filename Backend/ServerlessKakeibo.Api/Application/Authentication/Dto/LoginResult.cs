namespace ServerlessKakeibo.Api.Application.Authentication.Dto;

/// <summary>
/// ログイン結果
/// </summary>
/// <param name="AccessToken">アクセストークン（有効期限: 15分）</param>
/// <param name="RefreshToken">リフレッシュトークン（有効期限: 7日）</param>
/// <param name="UserId">ユーザーID</param>
/// <param name="DisplayName">表示名</param>
/// <param name="PictureUrl">プロフィール画像URL</param>
public record LoginResult(
    string AccessToken,
    string RefreshToken,
    Guid UserId,
    string DisplayName,
    string? PictureUrl
);
