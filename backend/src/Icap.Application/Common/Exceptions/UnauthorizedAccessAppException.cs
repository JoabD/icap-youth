namespace Icap.Application.Common.Exceptions;

/// <summary>
/// Excepción de aplicación para fallos de autenticación (ej. credenciales
/// inválidas en LoginCommand). Se traduce a 401 en el middleware global.
/// Nombre propio para no chocar con System.UnauthorizedAccessException.
/// </summary>
public sealed class UnauthorizedAccessAppException : Exception
{
    public UnauthorizedAccessAppException(string message) : base(message) { }
}
