using Icap.Application.Users.DTOs;
using Icap.Domain.Aggregates;

namespace Icap.Application.Users.Mappers;

internal static class UserDtoMapper
{
    public static UserDto ToDto(this User user) => new(
        Id: user.Id,
        FullName: user.FullName,
        Email: user.Email.Value,
        Role: user.Role.ToString(),
        IsActive: user.IsActive);
}
