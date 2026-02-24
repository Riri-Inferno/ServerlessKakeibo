using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// 月次推移取得リクエスト
/// </summary>
public class GetMonthlyTrendRequest
{
    /// <summary>
    /// 取得する月数（デフォルト: 12）
    /// </summary>
    [Range(1, 24, ErrorMessage = "月数は1-24の範囲で指定してください")]
    public int Months { get; set; } = 12;

    /// <summary>
    /// 基準年（指定しない場合は現在時刻）
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// 基準月（指定しない場合は現在時刻）
    /// </summary>
    [Range(1, 12, ErrorMessage = "月は1-12の範囲で指定してください")]
    public int? Month { get; set; }
}
