using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using ServerlessKakeibo.Api.Common.Exceptions;
using ServerlessKakeibo.Api.Service.Interface;

namespace ServerlessKakeibo.Api.Service;

/// <summary>
/// パスワードハッシュサービス
/// PBKDF2アルゴリズムを使用してパスワードをハッシュ化する
/// </summary>
public class PasswordHashService : IPasswordHashService
{
    private readonly ILogger<PasswordHashService> _logger;

    // ハッシュ化パラメータ
    private const int SaltSize = 16; // 128 bit
    private const int HashSize = 32; // 256 bit
    private const int Iterations = 100000; // OWASP推奨値
    private const KeyDerivationPrf Algorithm = KeyDerivationPrf.HMACSHA256;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public PasswordHashService(ILogger<PasswordHashService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// パスワードをハッシュ化
    /// </summary>
    /// <param name="password">ハッシュ化する平文パスワード</param>
    /// <returns>ハッシュ化されたパスワード（Base64エンコード: salt + hash）</returns>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "パスワードがnullまたは空です",
                nameof(password));
        }

        try
        {
            // ランダムなsaltを生成
            var salt = GenerateSalt();

            // パスワードをハッシュ化
            var hash = HashPasswordWithSalt(password, salt);

            // salt + hash を結合してBase64エンコード
            var hashBytes = new byte[SaltSize + HashSize];
            Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltSize);
            Buffer.BlockCopy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }
        catch (Exception ex) when (ex is not CustomException)
        {
            _logger.LogError(ex, "パスワードのハッシュ化中にエラーが発生しました");
            throw new CustomException(
                new ExceptionType(
                    HttpStatusCode.InternalServerError,
                    "パスワードのハッシュ化に失敗しました"),
                ex);
        }
    }

    /// <summary>
    /// パスワードがハッシュと一致するか検証
    /// </summary>
    /// <param name="password">検証する平文パスワード</param>
    /// <param name="hashedPassword">ハッシュ化されたパスワード</param>
    /// <returns>一致する場合true、それ以外はfalse</returns>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "パスワードがnullまたは空です",
                nameof(password));
        }

        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            throw new ArgumentException(
                "ハッシュ化されたパスワードがnullまたは空です",
                nameof(hashedPassword));
        }

        try
        {
            // Base64デコード
            var hashBytes = Convert.FromBase64String(hashedPassword);

            // サイズチェック
            if (hashBytes.Length != SaltSize + HashSize)
            {
                _logger.LogWarning(
                    "ハッシュのサイズが不正です。Expected: {Expected}, Actual: {Actual}",
                    SaltSize + HashSize,
                    hashBytes.Length);
                return false;
            }

            // saltとhashを分離
            var salt = new byte[SaltSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);

            var storedHash = new byte[HashSize];
            Buffer.BlockCopy(hashBytes, SaltSize, storedHash, 0, HashSize);

            // 入力パスワードを同じsaltでハッシュ化
            var inputHash = HashPasswordWithSalt(password, salt);

            // タイミング攻撃対策として定数時間比較を使用
            return CryptographicOperations.FixedTimeEquals(storedHash, inputHash);
        }
        catch (FormatException ex)
        {
            _logger.LogWarning(ex, "不正なBase64フォーマットのハッシュです");
            return false;
        }
        catch (Exception ex) when (ex is not CustomException)
        {
            _logger.LogError(ex, "パスワード検証中にエラーが発生しました");
            throw new CustomException(
                new ExceptionType(
                    HttpStatusCode.InternalServerError,
                    "パスワードの検証に失敗しました"),
                ex);
        }
    }

    /// <summary>
    /// ランダムなsaltを生成
    /// </summary>
    private static byte[] GenerateSalt()
    {
        var salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }

    /// <summary>
    /// 指定されたsaltを使用してパスワードをハッシュ化
    /// </summary>
    private static byte[] HashPasswordWithSalt(string password, byte[] salt)
    {
        return KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: Algorithm,
            iterationCount: Iterations,
            numBytesRequested: HashSize);
    }
}
