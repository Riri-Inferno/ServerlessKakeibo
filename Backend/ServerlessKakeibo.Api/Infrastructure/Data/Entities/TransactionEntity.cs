using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// 取引(収入・支出)エンティティ
    /// </summary>
    public class TransactionEntity : BaseEntity
    {
        /// <summary>
        /// 取引種別（収入/支出）
        /// </summary>
        public TransactionType Type { get; set; } = TransactionType.Expense;

        /// <summary>
        /// 取引日時
        /// </summary>
        public DateTimeOffset? TransactionDate { get; set; }

        /// <summary>
        /// 取引金額合計
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? AmountTotal { get; set; }

        /// <summary>
        /// 通貨コード(ISO 4217)
        /// </summary>
        [MaxLength(3)]
        public string Currency { get; set; } = "JPY";

        /// <summary>
        /// 支払者
        /// </summary>
        [MaxLength(200)]
        public string? Payer { get; set; }

        /// <summary>
        /// 受取者(店舗名など)
        /// </summary>
        [MaxLength(200)]
        public string? Payee { get; set; }

        /// <summary>
        /// 支払方法
        /// </summary>
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        /// <summary>
        /// 領収書種別
        /// </summary>
        [MaxLength(50)]
        public string? ReceiptType { get; set; }

        /// <summary>
        /// LLMによる判定信頼度(0.0 - 1.0)
        /// </summary>
        [Column(TypeName = "decimal(5,4)")]
        public decimal? Confidence { get; set; }

        /// <summary>
        /// 解析ステータス
        /// </summary>
        [MaxLength(50)]
        public string? ParseStatus { get; set; }

        /// <summary>
        /// 警告メッセージ(JSON配列)
        /// </summary>
        public string? WarningsJson { get; set; }

        /// <summary>
        /// 欠落フィールド(JSON配列)
        /// </summary>
        public string? MissingFieldsJson { get; set; }

        /// <summary>
        /// 元帳票の生データ(JSON)
        /// </summary>
        public string? RawDataJson { get; set; }

        /// <summary>
        /// カテゴリ(Enum)
        /// </summary>
        public TransactionCategory Category { get; set; } = TransactionCategory.Uncategorized;

        /// <summary>
        /// ユーザーID(外部キー)
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// ユーザー(ナビゲーションプロパティ)
        /// </summary>
        public UserEntity User { get; set; } = default!;

        /// <summary>
        /// 税情報一覧
        /// </summary>
        public ICollection<TaxDetailEntity> Taxes { get; set; }
            = new List<TaxDetailEntity>();

        /// <summary>
        /// 取引項目一覧
        /// </summary>
        public ICollection<TransactionItemEntity> Items { get; set; }
            = new List<TransactionItemEntity>();

        /// <summary>
        /// 店舗詳細情報
        /// </summary>
        public ShopDetailEntity? ShopDetail { get; set; }
    }
}
