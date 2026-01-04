using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// 店舗詳細情報エンティティ
    /// </summary>
    public class ShopDetailEntity : BaseEntity
    {
        /// <summary>
        /// 店舗名
        /// </summary>
        [MaxLength(200)]
        public string? Name { get; set; }

        /// <summary>
        /// 支店名
        /// </summary>
        [MaxLength(200)]
        public string? Branch { get; set; }

        /// <summary>
        /// 郵便番号
        /// </summary>
        [MaxLength(20)]
        public string? PostalCode { get; set; }

        /// <summary>
        /// 住所
        /// </summary>
        [MaxLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// 電話番号
        /// </summary>
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// 取引ID（外部キー・1対1）
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// 取引（ナビゲーションプロパティ）
        /// </summary>
        public TransactionEntity Transaction { get; set; } = default!;
    }
}
