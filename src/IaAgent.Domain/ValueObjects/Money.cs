using IaAgent.Domain.Enums;

namespace IaAgent.Domain.ValueObjects;

public sealed record Money(decimal Amount, Currency Currency)
{
    public static Money Zero(Currency currency) => new(0m, currency);
    public static Money InUsd(decimal amount) => new(amount, Currency.USD);
    public static Money InClp(decimal amount) => new(amount, Currency.CLP);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {Currency} and {other.Currency}");
        return this with { Amount = Amount + other.Amount };
    }

    public Money Multiply(decimal factor) => this with { Amount = Amount * factor };

    public Money ConvertTo(Currency target, decimal rate)
    {
        if (Currency == target) return this;
        return new Money(Amount * rate, target);
    }

    public override string ToString() => Currency == Currency.CLP
        ? $"${Amount:N0} CLP"
        : $"${Amount:N2} USD";
}
