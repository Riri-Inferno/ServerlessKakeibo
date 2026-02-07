using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// 取引カテゴリマスタ（システム提供）
    /// </summary>
    public class TransactionCategoryMasterEntity : BaseEntity
    {
        /// <summary>
        /// カテゴリ名（日本語） 例: "食費"
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// カテゴリコード（英語） 例: "Food"
        /// LLM判定用、システム内部識別用
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 表示色（HEX形式） 例: "#FF5733"
        /// </summary>
        [Required]
        [MaxLength(7)]
        public string ColorCode { get; set; } = "#808080";

        /// <summary>
        /// 表示順序（昇順）
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// 収入カテゴリか（false=支出、true=収入）
        /// </summary>
        public bool IsIncome { get; set; }

        /// <summary>
        /// システム提供のデフォルトカテゴリか
        /// </summary>
        public bool IsSystemDefault { get; set; } = true;

        /// <summary>
        /// このマスタを元にしたユーザーカテゴリ一覧
        /// </summary>
        public ICollection<UserTransactionCategoryEntity> UserCategories { get; set; }
            = new List<UserTransactionCategoryEntity>();
    }
}
