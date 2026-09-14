namespace Icap.Domain.Services;

/// <summary>
/// Contrato para generar el número de folio único y secuencial/alfanumérico
/// de un nuevo recibo. La estrategia concreta (contador atómico en Mongo,
/// timestamp + random, etc.) es un detalle de Infrastructure.
/// </summary>
public interface IFolioNumberGenerator
{
    Task<string> NextAsync(CancellationToken cancellationToken = default);
}
