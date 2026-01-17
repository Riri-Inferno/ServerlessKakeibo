using ServerlessKakeibo.Api.Application.Authentication.Dto;

namespace ServerlessKakeibo.Api.Application.Authentication.Usecases;

/// <summary>
/// トークン更新ユースケース
/// </summary>
public interface IRefreshTokenUseCase
{
    /// <summary>
    /// リフレッシュトークンを使って新しいアクセストークンを取得
    /// </summary>
    /// <param name="refreshToken">リフレッシュトークン</param>
    /// <param name="cancellationToken">キャンセレーショントークン</param>
    /// <returns>新しいアクセストークンとリフレッシュトークン</returns>
    Task<LoginResult> ExecuteAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}
