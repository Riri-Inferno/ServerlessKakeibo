using ServerlessKakeibo.Api.Application.UserSettings.Dto;
using ServerlessKakeibo.Api.Contracts;

namespace ServerlessKakeibo.Api.Application.UserSettings.Usecases;

/// <summary>
/// ユーザー設定更新ユースケース
/// </summary>
public interface IUpdateUserSettingsUseCase
{
    /// <summary>
    /// ユーザー設定を更新
    /// </summary>
    /// <param name="userId">ユーザーID</param>
    /// <param name="request">更新リクエスト</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>更新結果</returns>
    Task<UpdateUserSettingsResult> ExecuteAsync(
        Guid userId,
        UpdateUserSettingsRequest request,
        CancellationToken cancellationToken = default);
}
