using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// トークン更新リクエスト
/// </summary>
public class RefreshTokenRequest
{
    /// <summary>
    /// リフレッシュトークン
    /// </summary>
    [Required(ErrorMessage = "リフレッシュトークンは必須です")]
    public string RefreshToken { get; set; } = string.Empty;
}
