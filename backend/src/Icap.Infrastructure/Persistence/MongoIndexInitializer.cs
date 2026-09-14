using Icap.Infrastructure.Persistence.Documents;
using MongoDB.Driver;

namespace Icap.Infrastructure.Persistence;

/// <summary>
/// Crea los índices requeridos por el Documento de Diseño del Sistema
/// ("Email, Indexed, Unique" en Users; "FolioNumber, Indexed" en Receipts).
/// `CreateIndexAsync` es idempotente: puede llamarse en cada arranque sin
/// duplicar índices existentes.
/// </summary>
public static class MongoIndexInitializer
{
    public static async Task EnsureIndexesAsync(IMongoDbContext context, CancellationToken cancellationToken = default)
    {
        var emailIndex = new CreateIndexModel<UserDocument>(
            Builders<UserDocument>.IndexKeys.Ascending(u => u.Email),
            new CreateIndexOptions { Unique = true, Name = "ux_users_email" });

        await context.Users.Indexes.CreateOneAsync(emailIndex, cancellationToken: cancellationToken);

        var folioIndex = new CreateIndexModel<ReceiptDocument>(
            Builders<ReceiptDocument>.IndexKeys.Ascending(r => r.FolioNumber),
            new CreateIndexOptions { Unique = true, Name = "ux_receipts_folionumber" });

        var createdByIndex = new CreateIndexModel<ReceiptDocument>(
            Builders<ReceiptDocument>.IndexKeys.Ascending(r => r.CreatedBy),
            new CreateIndexOptions { Name = "ix_receipts_createdby" });

        await context.Receipts.Indexes.CreateManyAsync(
            new[] { folioIndex, createdByIndex },
            cancellationToken);
    }
}
