namespace CommandDeck.SharedKernel.Domain;

public abstract class SingleValueObject<T> : ValueObject
{
    protected SingleValueObject(T value)
    {
        Value = value;
    }

    public T Value { get; }

    public override string? ToString()
    {
        return Value?.ToString();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
