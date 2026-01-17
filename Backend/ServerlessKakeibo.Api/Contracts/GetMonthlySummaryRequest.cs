using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// 月次サマリー取得リクエスト
/// </summary>
public class GetMonthlySummaryRequest
{
    /// <summary>
    /// 対象年
    /// </summary>
    [Required(ErrorMessage = "年は必須です")]
    [Range(2000, 2100, ErrorMessage = "年は2000-2100の範囲で指定してください")]
    public int Year { get; set; }

    /// <summary>
    /// 対象月（1-12）
    /// </summary>
    [Required(ErrorMessage = "月は必須です")]
    [Range(1, 12, ErrorMessage = "月は1-12の範囲で指定してください")]
    public int Month { get; set; }
}
