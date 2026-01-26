namespace ServerlessKakeibo.Api.Application.UserSettings.Dto;

/// <summary>
/// ユーザー設定のレスポンスDTO
/// </summary>
public record UserSettingsDto
{
    /// <summary>
    /// 表示名(DisplayNameOverrideがあればそれ、なければGoogle由来)
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// メールアドレス
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// プロフィール画像URL
    /// </summary>
    public string? PictureUrl { get; init; }

    /// <summary>
    /// 締め日(1-31, nullは月末締め)
    /// </summary>
    public int? ClosingDay { get; init; }

    /// <summary>
    /// タイムゾーン(IANA形式)
    /// </summary>
    public string TimeZone { get; init; } = "Asia/Tokyo";

    /// <summary>
    /// 通貨コード(ISO 4217)
    /// </summary>
    public string CurrencyCode { get; init; } = "JPY";

    /// <summary>
    /// カスタマイズした表示名
    /// nullの場合はGoogle由来の名前を使用している状態
    /// フロントで「Google情報に戻す」機能を実装するために必要
    /// </summary>
    public string? DisplayNameOverride { get; init; }
}
