using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Service.Interface;

/// <summary>
/// JWTトークン発行サービス
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// アクセストークンを生成
    /// </summary>
    /// <param name="user">ユーザー情報</param>
    /// <returns>JWTアクセストークン</returns>
    string GenerateToken(UserEntity user);

    /// <summary>
    /// リフレッシュトークンを生成
    /// </summary>
    /// <returns>ランダムなリフレッシュトークン文字列</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// リフレッシュトークンの有効期限（日数）
    /// </summary>
    int RefreshTokenExpirationDays { get; }
}
