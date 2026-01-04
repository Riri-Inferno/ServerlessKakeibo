using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities;

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
    /// 税種別（消費税、入湯税、宿泊税等）
    /// </summary>
    [MaxLength(50)]
    public string TaxType { get; set; } = "消費税";

    /// <summary>
    /// 固定額の税金かどうか
    /// </summary>
    public bool IsFixedAmount { get; set; }

    /// <summary>
    /// 適用される項目カテゴリ（軽減税率対象等）
    /// </summary>
    [MaxLength(100)]
    public string? ApplicableCategory { get; set; }

    /// <summary>
    /// 取引ID（外部キー）
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// 取引（ナビゲーションプロパティ）
    /// </summary>
    public TransactionEntity Transaction { get; set; } = default!;
}
