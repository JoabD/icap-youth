using Icap.Application.Users.DTOs;
using MediatR;

namespace Icap.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserDto>>;
