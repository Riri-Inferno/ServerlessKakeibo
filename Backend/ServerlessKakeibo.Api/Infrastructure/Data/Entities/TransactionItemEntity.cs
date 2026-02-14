using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// 取引項目エンティティ
    /// </summary>
    public class TransactionItemEntity : BaseEntity
    {
        /// <summary>
        /// ユーザー商品カテゴリID（支出用、外部キー）
        /// </summary>
        public Guid? UserItemCategoryId { get; set; }

        /// <summary>
        /// 項目名
        /// </summary>
        [MaxLength(200)]
        public string? Name { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        [Column(TypeName = "decimal(18,3)")]
        public decimal? Quantity { get; set; }

        /// <summary>
        /// 単価
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? Amount { get; set; }

        /// <summary>
        /// 商品カテゴリ（LLMが推論）
        /// TODO:カスタムカテゴリに置き換え後削除する
        /// </summary>
        public ItemCategory Category { get; set; } = ItemCategory.Uncategorized;

        /// <summary>
        /// 取引ID（外部キー）
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// 取引（ナビゲーションプロパティ）
        /// </summary>
        public TransactionEntity Transaction { get; set; } = default!;

        /// <summary>
        /// ユーザー商品カテゴリ（支出用、ナビゲーションプロパティ）
        /// </summary>
        public UserItemCategoryEntity? UserItemCategory { get; set; }

        // 【追加】新しい外部キー（収入用）
        /// <summary>
        /// ユーザー給与項目カテゴリID（収入用、外部キー）
        /// </summary>
        public Guid? UserIncomeItemCategoryId { get; set; }

        /// <summary>
        /// ユーザー給与項目カテゴリ（収入用、ナビゲーションプロパティ）
        /// </summary>
        public UserIncomeItemCategoryEntity? UserIncomeItemCategory { get; set; }
    }
}
