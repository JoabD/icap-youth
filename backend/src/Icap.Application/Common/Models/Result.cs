namespace Icap.Application.Common.Models;

/// <summary>
/// Envoltorio estándar de resultado para Commands/Queries, evita usar
/// excepciones para flujo de control esperado (ej. "credenciales inválidas").
/// </summary>
public class Result
{
    public bool Succeeded { get; }
    public string[] Errors { get; }

    protected Result(bool succeeded, IEnumerable<string>? errors = null)
    {
        Succeeded = succeeded;
        Errors = errors?.ToArray() ?? Array.Empty<string>();
    }

    public static Result Success() => new(true);

    public static Result Failure(params string[] errors) => new(false, errors);
}

/// <summary>Variante de <see cref="Result"/> que además transporta un valor de retorno.</summary>
public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool succeeded, T? value, IEnumerable<string>? errors = null)
        : base(succeeded, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value);

    public static new Result<T> Failure(params string[] errors) => new(false, default, errors);
}
