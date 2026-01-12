using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    /// JWTトークンを生成
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public string GenerateToken(UserEntity user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(JwtRegisteredClaimNames.Name, user.DisplayName), // ← 追加推奨
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var secretKey = _config["Authentication:Jwt:SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey が設定されていません");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = _config["Authentication:Jwt:Issuer"];
        var audience = _config["Authentication:Jwt:Audience"];
        var expirationMinutes = int.Parse(_config["Authentication:Jwt:ExpirationMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
