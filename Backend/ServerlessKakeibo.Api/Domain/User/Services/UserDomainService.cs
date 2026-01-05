namespace ServerlessKakeibo.Api.Domain.User.Services;

/// <summary>
/// ユーザードメインサービス
/// </summary>
public class UserDomainService
{
    /// <summary>
    /// ユーザーの妥当性検証
    /// </summary>
    public bool ValidateUser(Models.User user)
    {
        return user.IsDisplayNameValid() && user.IsEmailValid();
    }

    /// <summary>
    /// 重複チェック（外部リポジトリとの協調が必要）
    /// </summary>
    public bool CanCreateUser(string email, Func<string, Task<bool>> emailExistsCheck)
    {
        if (string.IsNullOrWhiteSpace(email))
            return true;

        return !emailExistsCheck(email).Result;
    }
}
