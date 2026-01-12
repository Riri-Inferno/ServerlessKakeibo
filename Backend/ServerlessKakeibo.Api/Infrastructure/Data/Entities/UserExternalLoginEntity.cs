using System.ComponentModel.DataAnnotations;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// 外部認証（OAuth2/OpenID Connect）の紐付け管理エンティティ
    /// </summary>
    public class UserExternalLoginEntity : BaseEntity
    {
        /// <summary>
        /// 内部ユーザーIDへの外部キー
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 認証プロバイダー名（例: "Google", "GitHub"）
        /// </summary>
        [Required]
        [MaxLength(50)]
        public AuthProvider ProviderName { get; set; } = AuthProvider.None;

        /// <summary>
        /// プロバイダー側での一意なユーザー識別子
        /// Googleの場合、IDトークンの 'sub' クレームに該当
        /// </summary>
        [Required]
        [MaxLength(128)]
        public string ProviderKey { get; set; } = string.Empty;

        /// <summary>
        /// ユーザーエンティティへのナビゲーションプロパティ
        /// </summary>
        public UserEntity User { get; set; } = null!;
    }
}
