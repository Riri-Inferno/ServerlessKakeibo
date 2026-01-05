namespace ServerlessKakeibo.Api.Domain.ValueObjects;

public enum ErrorSeverity
{
    /// <summary>
    /// 致命的エラー(保存不可)
    /// </summary>
    Critical,

    /// <summary>
    /// 警告(保存可能だが要確認)
    /// </summary>
    Warning
}
