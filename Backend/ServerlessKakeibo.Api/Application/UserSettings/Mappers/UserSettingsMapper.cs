using ServerlessKakeibo.Api.Application.UserSettings.Dto;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Application.UserSettings.Mappers;

/// <summary>
/// ユーザー設定マッパー
/// </summary>
public static class UserSettingsMapper
{
    /// <summary>
    /// UserEntity + UserSettingsEntity → UserSettingsDto 変換
    /// </summary>
    /// <param name="user">ユーザーエンティティ</param>
    /// <param name="settings">設定エンティティ(nullの場合はデフォルト値を使用)</param>
    /// <returns>ユーザー設定DTO</returns>
    public static UserSettingsDto ToDto(UserEntity user, UserSettingsEntity? settings)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // DisplayName: DisplayNameOverrideがあればそれ、なければUserの値
        var displayName = !string.IsNullOrWhiteSpace(settings?.DisplayNameOverride)
            ? settings.DisplayNameOverride
            : user.DisplayName;

        return new UserSettingsDto
        {
            DisplayName = displayName,
            Email = user.Email,
            PictureUrl = user.PictureUrl,
            ClosingDay = settings?.ClosingDay,
            TimeZone = settings?.TimeZone ?? "Asia/Tokyo",
            CurrencyCode = settings?.CurrencyCode ?? "JPY",
            DisplayNameOverride = settings?.DisplayNameOverride
        };
    }

    /// <summary>
    /// UserSettingsEntity → UserSettingsDto 変換
    /// (UserがIncludeされている前提)
    /// </summary>
    /// <param name="settings">設定エンティティ(User情報を含む)</param>
    /// <returns>ユーザー設定DTO</returns>
    public static UserSettingsDto ToDto(UserSettingsEntity settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        if (settings.User == null)
            throw new InvalidOperationException("UserSettingsEntity.User がロードされていません。Include(us => us.User)を使用してください。");

        return ToDto(settings.User, settings);
    }
}
