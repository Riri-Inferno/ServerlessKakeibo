using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Service.Interface;

namespace ServerlessKakeibo.Api.Service;

/// <summary>
/// JWTトークン発行サービス
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config) => _config = config;

    /// <summary>
    /// リフレッシュトークンの有効期限（日数）
    /// </summary>
    public int RefreshTokenExpirationDays =>
        int.Parse(_config["Authentication:Jwt:RefreshTokenExpirationDays"] ?? "7");

    /// <summary>
    /// アクセストークンを生成
    /// </summary>
    public string GenerateToken(UserEntity user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var secretKey = _config["Authentication:Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey が設定されていません");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = _config["Authentication:Jwt:Issuer"];
        var audience = _config["Authentication:Jwt:Audience"];
        var expirationMinutes = int.Parse(_config["Authentication:Jwt:ExpirationMinutes"] ?? "15");

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// リフレッシュトークンを生成（ランダムな256ビット文字列）
    /// </summary>
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
