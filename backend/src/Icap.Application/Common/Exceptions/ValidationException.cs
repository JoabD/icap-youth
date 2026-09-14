using FluentValidation.Results;

namespace Icap.Application.Common.Exceptions;

/// <summary>
/// Se lanza desde el ValidationBehavior cuando un Command/Query no pasa
/// las reglas de FluentValidation. El middleware global de Presentation
/// la traduce a un 400 con el detalle de errores por campo.
/// </summary>
public sealed class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException()
        : base("Se encontraron uno o más errores de validación.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}
