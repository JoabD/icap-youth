using System.Text.RegularExpressions;
using Icap.Domain.Common;
using Icap.Domain.Exceptions;

namespace Icap.Domain.ValueObjects;

/// <summary>
/// Value Object para direcciones de correo. Garantiza formato válido
/// y normalización (lowercase) en un único punto del dominio.
/// </summary>
public sealed partial class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Of(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El correo electrónico es requerido.");

        var normalized = value.Trim().ToLowerInvariant();

        if (!EmailRegex().IsMatch(normalized))
            throw new DomainException($"El correo electrónico '{value}' no tiene un formato válido.");

        return new Email(normalized);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)]
    private static partial Regex EmailRegex();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
