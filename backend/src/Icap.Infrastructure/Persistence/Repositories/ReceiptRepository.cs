using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using Icap.Infrastructure.Persistence.Documents;
using Icap.Infrastructure.Persistence.Mappers;
using MongoDB.Driver;

namespace Icap.Infrastructure.Persistence.Repositories;

public sealed class ReceiptRepository : IReceiptRepository
{
    private readonly IMongoCollection<ReceiptDocument> _receipts;

    public ReceiptRepository(IMongoDbContext context)
    {
        _receipts = context.Receipts;
    }

    public async Task<Receipt?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var document = await _receipts
            .Find(r => r.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document?.ToDomain();
    }

    public async Task<Receipt?> GetByFolioNumberAsync(string folioNumber, CancellationToken cancellationToken = default)
    {
        var document = await _receipts
            .Find(r => r.FolioNumber == folioNumber)
            .FirstOrDefaultAsync(cancellationToken);

        return document?.ToDomain();
    }

    public Task<long> CountAllAsync(CancellationToken cancellationToken = default) =>
        _receipts.CountDocumentsAsync(FilterDefinition<ReceiptDocument>.Empty, cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<Receipt>> GetByCreatedByAsync(string createdByUserId, CancellationToken cancellationToken = default)
    {
        var documents = await _receipts
            .Find(r => r.CreatedBy == createdByUserId)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<Receipt>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _receipts
            .Find(FilterDefinition<ReceiptDocument>.Empty)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default) =>
        _receipts.InsertOneAsync(receipt.ToDocument(), cancellationToken: cancellationToken);

    public Task UpdateAsync(Receipt receipt, CancellationToken cancellationToken = default) =>
        _receipts.ReplaceOneAsync(r => r.Id == receipt.Id, receipt.ToDocument(), cancellationToken: cancellationToken);
}
