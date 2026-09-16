using MediatR;

namespace Icap.Application.Users.Commands.ActivateUser;

public sealed record ActivateUserCommand(string Id) : IRequest;
