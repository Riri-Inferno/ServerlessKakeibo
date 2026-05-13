using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// リフレッシュトークンエンティティ
    /// 端末ごとに1レコード発行し、ローテーション時は旧レコードを論理削除して新レコードを挿入する。
    /// </summary>
    public class RefreshTokenEntity : BaseEntity
    {
        /// <summary>
        /// 紐づくユーザーID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// リフレッシュトークンのハッシュ値（PBKDF2 によるハッシュ）
        /// Base64エンコードされた salt + hash (最大約64文字)
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>
        /// リフレッシュトークンの有効期限
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// 紐づくユーザー
        /// </summary>
        public UserEntity User { get; set; } = null!;
    }
}
