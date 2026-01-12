using System.ComponentModel;

namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// 認証プロバイダ
/// </summary>
public enum AuthProvider
{
    /// <summary>
    /// 未指定または不明なプロバイダ
    /// </summary>
    [Description("未指定")]
    None = 0,

    /// <summary>
    /// Google認証
    /// </summary>
    [Description("Google")]
    Google = 1,
}

/// <summary>
/// AuthProvider 拡張メソッド
/// </summary>
public static class AuthProviderExtensions
{
    /// <summary>
    /// 表示名（日本語）を取得
    /// </summary>
    public static string ToJapanese(this AuthProvider provider)
    {
        var type = typeof(AuthProvider);
        var memberInfo = type.GetMember(provider.ToString());

        if (memberInfo.Length > 0)
        {
            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return ((DescriptionAttribute)attributes[0]).Description;
            }
        }

        return provider.ToString();
    }

    /// <summary>
    /// 日本語名からEnumに変換
    /// </summary>
    /// <param name="providerName">プロバイダの日本語表示名</param>
    public static AuthProvider? FromJapanese(string? providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return null;

        return providerName switch
        {
            "Google" => AuthProvider.Google,
            "未指定" => AuthProvider.None,
            _ => null
        };
    }
}
