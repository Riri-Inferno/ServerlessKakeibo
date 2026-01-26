using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// ユーザー設定更新リクエスト
/// </summary>
public record UpdateUserSettingsRequest
{
    /// <summary>
    /// 表示名の上書き
    /// - null: 変更なし
    /// - 空文字列: Google情報に戻す
    /// - 値あり: この値を設定
    /// </summary>
    [MaxLength(100, ErrorMessage = "表示名は100文字以内で入力してください")]
    public string? DisplayNameOverride { get; init; }

    /// <summary>
    /// 締め日(1-31)
    /// nullの場合は月末締め
    /// </summary>
    [Range(1, 31, ErrorMessage = "締め日は1〜31の範囲で指定してください")]
    public int? ClosingDay { get; init; }

    /// <summary>
    /// タイムゾーン(IANA形式, 例: "Asia/Tokyo")
    /// </summary>
    [MaxLength(50, ErrorMessage = "タイムゾーンは50文字以内で入力してください")]
    public string? TimeZone { get; init; }

    /// <summary>
    /// 通貨コード(ISO 4217, 例: "JPY")
    /// </summary>
    [MaxLength(3, ErrorMessage = "通貨コードは3文字で入力してください")]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "通貨コードは3文字の大文字アルファベットで入力してください")]
    public string? CurrencyCode { get; init; }
}
