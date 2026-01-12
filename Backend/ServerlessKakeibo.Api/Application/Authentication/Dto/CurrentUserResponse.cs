namespace ServerlessKakeibo.Api.Application.Authentication.Dto;

/// <summary>
/// 現在のユーザー情報レスポンス
/// </summary>
public record CurrentUserResponse(
    Guid UserId,
    string DisplayName,
    string? Email
);
