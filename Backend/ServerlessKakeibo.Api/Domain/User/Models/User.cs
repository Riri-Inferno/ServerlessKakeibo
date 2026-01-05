namespace ServerlessKakeibo.Api.Domain.User.Models;

/// <summary>
/// ユーザードメインモデル
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Guid TenantId { get; set; }

    /// <summary>
    /// ドメインルール：メールアドレスの妥当性チェック
    /// </summary>
    public bool IsEmailValid()
    {
        if (string.IsNullOrWhiteSpace(Email))
            return true; // Nullは許容

        return Email.Contains('@') && Email.Length >= 5;
    }

    /// <summary>
    /// ドメインルール：表示名の妥当性チェック
    /// </summary>
    public bool IsDisplayNameValid()
    {
        return !string.IsNullOrWhiteSpace(DisplayName) && DisplayName.Length <= 100;
    }
}
