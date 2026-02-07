using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// 商品カテゴリマスタ（支出用、システム提供）
    /// </summary>
    public class ItemCategoryMasterEntity : BaseEntity
    {
        /// <summary>
        /// カテゴリ名（日本語） 例: "食品"
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
        /// 表示色（HEX形式） 例: "#4CAF50"
        /// </summary>
        [Required]
        [MaxLength(7)]
        public string ColorCode { get; set; } = "#808080";

        /// <summary>
        /// 表示順序（昇順）
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// システム提供のデフォルトカテゴリか
        /// </summary>
        public bool IsSystemDefault { get; set; } = true;

        /// <summary>
        /// このマスタを元にしたユーザーカテゴリ一覧
        /// </summary>
        public ICollection<UserItemCategoryEntity> UserCategories { get; set; }
            = new List<UserItemCategoryEntity>();
    }
}
