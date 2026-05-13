using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// APIキーエンティティ
    /// ユーザー単位で発行する長寿命のキー。ハッシュ値で保存し、プレフィックスで識別する。
    /// </summary>
    public class ApiKeyEntity : BaseEntity
    {
        /// <summary>
        /// 紐づくユーザーID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// ユーザーが付けるラベル（例: "MCP - 自宅PC"）
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// キーの先頭プレフィックス（"slk_" + 8文字）
        /// UI 表示と認証時の候補絞り込みに使用する。平文保存。
        /// </summary>
        [Required]
        [MaxLength(16)]
        public string KeyPrefix { get; set; } = string.Empty;

        /// <summary>
        /// キー本体の PBKDF2 ハッシュ（Base64エンコード salt + hash）
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string KeyHash { get; set; } = string.Empty;

        /// <summary>
        /// スコープ集合（スペース区切り）。例: "read"、"read write"
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string Scopes { get; set; } = string.Empty;

        /// <summary>
        /// 有効期限。NULL の場合は無期限
        /// 自己責任
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// 最終使用日時。認証成功時に更新
        /// </summary>
        public DateTimeOffset? LastUsedAt { get; set; }

        /// <summary>
        /// 失効日時。NULL でなければ認証時に拒否される
        /// </summary>
        public DateTimeOffset? RevokedAt { get; set; }

        /// <summary>
        /// 紐づくユーザー
        /// </summary>
        public UserEntity User { get; set; } = null!;
    }
}
