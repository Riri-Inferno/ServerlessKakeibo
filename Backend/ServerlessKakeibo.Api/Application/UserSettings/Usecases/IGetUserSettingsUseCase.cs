using ServerlessKakeibo.Api.Application.UserSettings.Dto;

namespace ServerlessKakeibo.Api.Application.UserSettings.Usecases;

/// <summary>
/// ユーザー設定取得ユースケース
/// </summary>
public interface IGetUserSettingsUseCase
{
    /// <summary>
    /// ユーザー設定を取得
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>ユーザー設定DTO</returns>
    Task<UserSettingsDto> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
