using Icap.Infrastructure.Persistence.Documents;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Icap.Infrastructure.Persistence;

/// <summary>
/// Abstracción sobre las colecciones de Mongo usadas por los repositorios,
/// para poder mockearla fácilmente en pruebas unitarias de Infrastructure.
/// </summary>
public interface IMongoDbContext
{
    IMongoCollection<UserDocument> Users { get; }
    IMongoCollection<ReceiptDocument> Receipts { get; }

    /// <summary>Colección genérica de contadores atómicos (ej. folios secuenciales).</summary>
    IMongoCollection<BsonDocument> Counters { get; }
}
