namespace Icap.Domain.Exceptions;

/// <summary>
/// Excepción base para violaciones de reglas de negocio del dominio.
/// La capa de Presentation la traduce a un 400/422 vía el middleware global.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}
