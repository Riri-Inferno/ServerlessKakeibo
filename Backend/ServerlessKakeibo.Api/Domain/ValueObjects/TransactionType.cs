using System.ComponentModel;

namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// 取引種別（収入/支出）
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// 支出
    /// </summary>
    [Description("支出")]
    Expense = 0,

    /// <summary>
    /// 収入
    /// </summary>
    [Description("収入")]
    Income = 1
}
