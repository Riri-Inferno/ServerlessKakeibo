namespace ServerlessKakeibo.Api.Application.Statistics.Dto;

/// <summary>
/// データ範囲結果
/// </summary>
public class DateRangeResult
{
    /// <summary>
    /// 最古の取引年
    /// </summary>
    public int? OldestYear { get; set; }

    /// <summary>
    /// 最古の取引月（1-12）
    /// </summary>
    public int? OldestMonth { get; set; }

    /// <summary>
    /// 最新の取引年
    /// </summary>
    public int? NewestYear { get; set; }

    /// <summary>
    /// 最新の取引月（1-12）
    /// </summary>
    public int? NewestMonth { get; set; }
}
