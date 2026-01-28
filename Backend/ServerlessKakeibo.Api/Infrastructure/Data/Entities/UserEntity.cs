using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// ユーザーエンティティ
    /// </summary>
    public class UserEntity : BaseEntity
    {
        /// <summary>
        /// 表示名(Googleから取得した名前、またはユーザーが設定した名前)
        /// Googleログイン時に毎回更新される
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// メールアドレス
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; }

        /// <summary>
        /// プロフィール画像のURL(Googleから提供される画像URLなど)
        /// </summary>
        [MaxLength(500)]
        public string? PictureUrl { get; set; }

        /// <summary>
        /// リフレッシュトークンのハッシュ値(PBKDF2でハッシュ化されたトークン)
        /// Base64エンコードされたsalt+hash (最大約64文字)
        /// </summary>
        [MaxLength(100)]
        public string? RefreshTokenHash { get; set; }

        /// <summary>
        /// リフレッシュトークンの有効期限
        /// </summary>
        public DateTimeOffset? RefreshTokenExpiry { get; set; }

        /// <summary>
        /// 紐づいている外部認証情報の一覧
        /// </summary>
        public ICollection<UserExternalLoginEntity> ExternalLogins { get; set; }
            = new List<UserExternalLoginEntity>();

        /// <summary>
        /// このユーザーの取引一覧
        /// </summary>
        public ICollection<TransactionEntity> Transactions { get; set; }
            = new List<TransactionEntity>();

        /// <summary>
        /// このユーザーの設定(1対1リレーション)
        /// </summary>
        public UserSettingsEntity? Settings { get; set; }
    }
}
