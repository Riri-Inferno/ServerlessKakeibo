using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Service.Interface;

/// <summary>
/// JWTトークン発行サービス
/// </summary>
public interface IJwtTokenService
{
    string GenerateToken(UserEntity user);
}
