using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// ユーザーエンティティ（最低限版）
    /// </summary>
    public class UserEntity : BaseEntity
    {
        /// <summary>
        /// 表示名
        /// </summary>
        [MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// メールアドレス（将来の認証用）
        /// </summary>
        [MaxLength(256)]
        public string? Email { get; set; }

        /// <summary>
        /// このユーザーの取引一覧
        /// </summary>
        public ICollection<TransactionEntity> Transactions { get; set; }
            = new List<TransactionEntity>();
    }
}
