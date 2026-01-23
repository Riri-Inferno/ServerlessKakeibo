namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// トランザクション検証結果
/// </summary>
public class TransactionValidationResult
{
    public bool IsValid { get; set; }
    public List<TransactionValidationError> Errors { get; set; } = new();

    /// <summary>
    /// 成功結果を生成
    /// </summary>
    public static TransactionValidationResult Success()
    {
        return new TransactionValidationResult
        {
            IsValid = true,
            Errors = new List<TransactionValidationError>()
        };
    }

    /// <summary>
    /// 失敗結果を生成（単一エラー）
    /// </summary>
    public static TransactionValidationResult Failure(ErrorSeverity severity, string message)
    {
        return new TransactionValidationResult
        {
            IsValid = false,
            Errors = new List<TransactionValidationError>
            {
                new TransactionValidationError(message, severity)
            }
        };
    }
}

/// <summary>
/// トランザクション検証エラー
/// </summary>
public class TransactionValidationError
{
    public string Message { get; set; }
    public ErrorSeverity Severity { get; set; }

    public TransactionValidationError(string message, ErrorSeverity severity)
    {
        Message = message;
        Severity = severity;
    }
}

/// <summary>
/// 削除検証結果
/// </summary>
public class DeleteValidationResult
{
    public bool CanDelete { get; set; }
    public List<TransactionValidationError> Warnings { get; set; } = new();
}
