namespace ServerlessKakeibo.Api.Application.Authentication.Dto;

/// <summary>
/// ログイン結果
/// </summary>
/// <param name="AccessToken"></param>
/// <param name="UserId"></param>
/// <param name="DisplayName"></param>
/// <param name="PictureUrl"></param>
public record LoginResult(
    string AccessToken,
    Guid UserId,
    string DisplayName,
    string? PictureUrl
);
