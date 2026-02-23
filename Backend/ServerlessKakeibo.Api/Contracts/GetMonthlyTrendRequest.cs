using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// 月次推移取得リクエスト
/// </summary>
public class GetMonthlyTrendRequest
{
    /// <summary>
    /// 取得する月数（直近N ヶ月）
    /// </summary>
    [Range(1, 240, ErrorMessage = "月数は1から240の間で指定してください")]
    public int Months { get; set; } = 6;
}
