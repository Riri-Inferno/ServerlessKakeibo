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
        /// </summary>
        public ItemCategory Category { get; set; }

        /// <summary>
        /// 取引ID（外部キー）
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// 取引（ナビゲーションプロパティ）
        /// </summary>
        public TransactionEntity Transaction { get; set; } = default!;
    }
}
