using Icap.Infrastructure.Persistence.Documents;
using Icap.Infrastructure.Persistence.Settings;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Icap.Infrastructure.Persistence;

/// <summary>
/// Implementación concreta sobre MongoDB.Driver. Se registra como Singleton
/// en DI (IMongoClient es thread-safe y administra su propio pool de conexiones).
/// </summary>
public sealed class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
        var client = new MongoClient(_settings.ConnectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    public IMongoCollection<UserDocument> Users =>
        _database.GetCollection<UserDocument>(_settings.UsersCollectionName);

    public IMongoCollection<ReceiptDocument> Receipts =>
        _database.GetCollection<ReceiptDocument>(_settings.ReceiptsCollectionName);

    public IMongoCollection<BsonDocument> Counters =>
        _database.GetCollection<BsonDocument>(_settings.CountersCollectionName);
}
