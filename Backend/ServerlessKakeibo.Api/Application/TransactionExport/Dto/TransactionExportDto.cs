namespace ServerlessKakeibo.Api.Application.TransactionExport.Dto;

/// <summary>
/// CSV出力用取引DTO
/// </summary>
public class TransactionExportDto
{
    /// <summary>
    /// ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 取引種別
    /// </summary>
    public string 取引種別 { get; set; } = string.Empty;

    /// <summary>
    /// 取引日時
    /// </summary>
    public string 取引日時 { get; set; } = string.Empty;

    /// <summary>
    /// 金額
    /// </summary>
    public decimal? 金額 { get; set; }

    /// <summary>
    /// 通貨
    /// </summary>
    public string 通貨 { get; set; } = string.Empty;

    /// <summary>
    /// 支払者
    /// </summary>
    public string 支払者 { get; set; } = string.Empty;

    /// <summary>
    /// 受取者
    /// </summary>
    public string 受取者 { get; set; } = string.Empty;

    /// <summary>
    /// 支払方法
    /// </summary>
    public string 支払方法 { get; set; } = string.Empty;

    /// <summary>
    /// カテゴリ
    /// </summary>
    public string カテゴリ { get; set; } = string.Empty;

    /// <summary>
    /// 税区分
    /// </summary>
    public string 税区分 { get; set; } = string.Empty;

    /// <summary>
    /// メモ
    /// </summary>
    public string メモ { get; set; } = string.Empty;

    /// <summary>
    /// 明細件数
    /// </summary>
    public int 明細件数 { get; set; }

    /// <summary>
    /// 添付画像
    /// </summary>
    public string 添付画像 { get; set; } = string.Empty;

    /// <summary>
    /// 画像添付日時
    /// </summary>
    public string 画像添付日時 { get; set; } = string.Empty;

    /// <summary>
    /// 作成日時
    /// </summary>
    public string 作成日時 { get; set; } = string.Empty;

    /// <summary>
    /// 更新日時
    /// </summary>
    public string 更新日時 { get; set; } = string.Empty;
}
