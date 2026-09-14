using Icap.Domain.Common;
using Icap.Domain.Exceptions;

namespace Icap.Domain.ValueObjects;

/// <summary>
/// Value Object monetario. Encapsula el monto y la moneda para evitar
/// "primitive obsession" con decimales sueltos, y centraliza las reglas
/// de negocio (no negativos, redondeo, operaciones aritméticas seguras).
/// </summary>
public sealed class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    private const string DefaultCurrency = "MXN";

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Of(decimal amount, string currency = DefaultCurrency)
    {
        if (amount < 0)
            throw new DomainException("El monto (Money) no puede ser negativo.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new DomainException("La moneda (Currency) es requerida.");

        return new Money(decimal.Round(amount, 2, MidpointRounding.AwayFromZero), currency.ToUpperInvariant());
    }

    public static Money Zero(string currency = DefaultCurrency) => new(0m, currency);

    public Money Multiply(int factor)
    {
        if (factor < 0)
            throw new DomainException("El factor de multiplicación no puede ser negativo.");

        return Of(Amount * factor, Currency);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Of(Amount + other.Amount, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException($"No se pueden operar montos con distinta moneda ({Currency} vs {other.Currency}).");
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
