namespace Icap.Application.Common.Exceptions;

/// <summary>Se lanza cuando un Query/Command referencia una entidad inexistente. Se traduce a 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"'{entityName}' con identificador '{key}' no fue encontrado.") { }
}
