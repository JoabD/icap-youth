using Icap.Application.Users.DTOs;
using MediatR;

namespace Icap.Application.Users.Queries.GetUserById;

public sealed record GetUserByIdQuery(string Id) : IRequest<UserDto>;
