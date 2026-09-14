using Icap.Domain.Aggregates;

namespace Icap.Domain.Repositories;

/// <summary>
/// Contrato de persistencia para el Aggregate Receipt. La implementación
/// concreta (driver de MongoDB) vive en Infrastructure.
/// </summary>
public interface IReceiptRepository
{
    Task<Receipt?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<Receipt?> GetByFolioNumberAsync(string folioNumber, CancellationToken cancellationToken = default);

    /// <summary>Usado por Infrastructure para generar folios secuenciales únicos.</summary>
    Task<long> CountAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Receipt>> GetByCreatedByAsync(string createdByUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Receipt>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default);

    Task UpdateAsync(Receipt receipt, CancellationToken cancellationToken = default);
}
