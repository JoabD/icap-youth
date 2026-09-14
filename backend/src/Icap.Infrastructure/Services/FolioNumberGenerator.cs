using Icap.Domain.Services;
using Icap.Infrastructure.Persistence;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Icap.Infrastructure.Services;

/// <summary>
/// Genera folios secuenciales del tipo "ICAP-000123" usando un contador
/// atómico (findOneAndUpdate con $inc) en la colección "Counters", evitando
/// condiciones de carrera cuando dos delegados emiten recibos a la vez
/// (a diferencia de usar Receipts.CountAllAsync + 1, que sí tendría esa
/// condición de carrera bajo concurrencia).
/// </summary>
public sealed class FolioNumberGenerator : IFolioNumberGenerator
{
    private const string CounterId = "receipt_folio";
    private const string FolioPrefix = "ICAP";

    private readonly IMongoDbContext _context;

    public FolioNumberGenerator(IMongoDbContext context)
    {
        _context = context;
    }

    public async Task<string> NextAsync(CancellationToken cancellationToken = default)
    {
        var filter = Builders<BsonDocument>.Filter.Eq("_id", CounterId);
        // 1L (Int64) a propósito: si se deja como int (Int32), Mongo crea/incrementa
        // "sequence_value" como BsonInt32, y luego el cast estricto AsInt64 truena
        // con InvalidCastException. Con long, $inc siempre opera (y guarda) en Int64.
        var update = Builders<BsonDocument>.Update.Inc("sequence_value", 1L);
        var options = new FindOneAndUpdateOptions<BsonDocument>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After,
        };

        var counter = await _context.Counters.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        // ToInt64() (conversión) en vez de AsInt64 (cast estricto) a propósito:
        // así también funciona si el documento del contador ya quedó guardado como
        // Int32 por una corrida anterior (antes de este fix), sin exigir migrarlo.
        var nextValue = counter["sequence_value"].ToInt64();

        return $"{FolioPrefix}-{nextValue:D6}";
    }
}
