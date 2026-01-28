namespace ServerlessKakeibo.Api.Service.Interface;

/// <summary>
/// パスワードハッシュサービスのインターフェース
/// </summary>
public interface IPasswordHashService
{
    /// <summary>
    /// パスワードをハッシュ化
    /// </summary>
    /// <param name="password">ハッシュ化する平文パスワード</param>
    /// <returns>ハッシュ化されたパスワード</returns>
    /// <exception cref="ArgumentException">パスワードがnullまたは空の場合</exception>
    string HashPassword(string password);

    /// <summary>
    /// パスワードがハッシュと一致するか検証
    /// </summary>
    /// <param name="password">検証する平文パスワード</param>
    /// <param name="hashedPassword">ハッシュ化されたパスワード</param>
    /// <returns>一致する場合true、それ以外はfalse</returns>
    /// <exception cref="ArgumentException">パラメータがnullまたは空の場合</exception>
    bool VerifyPassword(string password, string hashedPassword);
}
