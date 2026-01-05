namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// 金額の値オブジェクト
/// </summary>
public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public Money(decimal amount, string currency = "JPY")
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }

    /// <summary>
    /// 金額の加算
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// 金額の減算
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot subtract different currencies");

        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// 金額のフォーマット
    /// </summary>
    public string Format()
    {
        return Currency switch
        {
            "JPY" => $"¥{Amount:N0}",
            "USD" => $"${Amount:N2}",
            _ => $"{Amount:N2} {Currency}"
        };
    }

    public override string ToString() => Format();
}
