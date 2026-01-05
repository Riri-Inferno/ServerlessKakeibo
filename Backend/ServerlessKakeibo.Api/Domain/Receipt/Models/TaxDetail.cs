namespace ServerlessKakeibo.Api.Domain.Receipt.Models;

/// <summary>
/// 税金詳細情報
/// </summary>
public class TaxDetail
{
    /// <summary>
    /// 税種別（消費税、入湯税、宿泊税等）
    /// </summary>
    public string TaxType { get; set; } = "消費税";

    /// <summary>
    /// 税率（パーセント）null の場合は固定額
    /// </summary>
    public int? TaxRate { get; set; }

    /// <summary>
    /// 税額
    /// </summary>
    public decimal? TaxAmount { get; set; }

    /// <summary>
    /// 課税対象額（税抜金額）
    /// </summary>
    public decimal? TaxableAmount { get; set; }

    /// <summary>
    /// 固定額の税金かどうか
    /// </summary>
    public bool IsFixedAmount { get; set; }

    /// <summary>
    /// 適用される項目カテゴリ（軽減税率対象等）
    /// </summary>
    public string? ApplicableCategory { get; set; }
}
