namespace ServerlessKakeibo.Api.Application.UserSettings.Dto;

/// <summary>
/// ユーザー設定更新の結果DTO
/// </summary>
public record UpdateUserSettingsResult
{
    /// <summary>
    /// 更新成功フラグ
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// 更新後の設定情報
    /// </summary>
    public UserSettingsDto? Settings { get; init; }

    /// <summary>
    /// エラーメッセージ(失敗時)
    /// </summary>
    public string? ErrorMessage { get; init; }
}
