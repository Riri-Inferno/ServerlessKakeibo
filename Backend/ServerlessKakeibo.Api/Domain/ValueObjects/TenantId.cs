namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// テナントIDの値オブジェクト
/// </summary>
public record TenantId
{
    public Guid Value { get; init; }

    public TenantId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("TenantId cannot be empty", nameof(value));

        Value = value;
    }

    public static TenantId NewId() => new(Guid.NewGuid());

    public static implicit operator Guid(TenantId tenantId) => tenantId.Value;
    public static implicit operator TenantId(Guid guid) => new(guid);

    public override string ToString() => Value.ToString();
}
