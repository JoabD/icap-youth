using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using Icap.Infrastructure.Persistence.Documents;
using Icap.Infrastructure.Persistence.Mappers;
using MongoDB.Driver;

namespace Icap.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IMongoCollection<UserDocument> _users;

    public UserRepository(IMongoDbContext context)
    {
        _users = context.Users;
    }

    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var document = await _users
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document?.ToDomain();
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var document = await _users
            .Find(u => u.Email == normalizedEmail)
            .FirstOrDefaultAsync(cancellationToken);

        return document?.ToDomain();
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _users
            .Find(u => u.Email == normalizedEmail)
            .AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _users.Find(FilterDefinition<UserDocument>.Empty).ToListAsync(cancellationToken);
        return documents.Select(d => d.ToDomain()).ToList();
    }

    public Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        _users.InsertOneAsync(user.ToDocument(), cancellationToken: cancellationToken);

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default) =>
        _users.ReplaceOneAsync(u => u.Id == user.Id, user.ToDocument(), cancellationToken: cancellationToken);
}
