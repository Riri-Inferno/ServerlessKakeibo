using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// 取引エクスポートリクエスト
/// </summary>
public class ExportTransactionsRequest : GetTransactionsRequest
{
    /// <summary>
    /// 添付画像を含めるか（true = Zip with images, false = CSV only in Zip）
    /// </summary>
    public bool IncludeReceiptImages { get; set; } = false;
}
