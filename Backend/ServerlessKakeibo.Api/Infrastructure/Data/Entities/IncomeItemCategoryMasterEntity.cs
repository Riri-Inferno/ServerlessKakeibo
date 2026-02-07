using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// 給与項目カテゴリマスタ（収入用、システム提供）
    /// </summary>
    public class IncomeItemCategoryMasterEntity : BaseEntity
    {
        /// <summary>
        /// 項目名（日本語） 例: "基本給"
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 項目コード（英語） 例: "BaseSalary"
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
        public ICollection<UserIncomeItemCategoryEntity> UserCategories { get; set; }
            = new List<UserIncomeItemCategoryEntity>();
    }
}
