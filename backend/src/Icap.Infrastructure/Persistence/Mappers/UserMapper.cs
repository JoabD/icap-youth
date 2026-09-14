using Icap.Domain.Aggregates;
using Icap.Domain.Enums;
using Icap.Infrastructure.Persistence.Documents;

namespace Icap.Infrastructure.Persistence.Mappers;

internal static class UserMapper
{
    public static User ToDomain(this UserDocument document) => User.Rehydrate(
        id: document.Id,
        fullName: document.FullName,
        email: document.Email,
        passwordHash: document.PasswordHash,
        role: Enum.Parse<UserRole>(document.Role),
        isActive: document.IsActive,
        extraAttributes: document.ExtraAttributes);

    public static UserDocument ToDocument(this User user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email.Value,
        PasswordHash = user.PasswordHash,
        Role = user.Role.ToString(),
        IsActive = user.IsActive,
        ExtraAttributes = new Dictionary<string, object>(user.ExtraAttributes),
    };
}
