using System.ComponentModel.DataAnnotations.Schema;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// 税情報エンティティ
    /// </summary>
    public class TaxDetailEntity : BaseEntity
    {
        /// <summary>
        /// 税率（パーセンテージ）
        /// </summary>
        public int? TaxRate { get; set; }

        /// <summary>
        /// 税額
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TaxAmount { get; set; }

        /// <summary>
        /// 対象金額（税抜）
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? TaxableAmount { get; set; }

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
