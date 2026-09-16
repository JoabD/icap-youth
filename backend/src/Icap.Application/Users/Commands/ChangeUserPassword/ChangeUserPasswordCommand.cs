using MediatR;

namespace Icap.Application.Users.Commands.ChangeUserPassword;

/// <summary>Restablece la contraseña de un usuario. Uso administrativo (un Admin resetea la contraseña de un delegado, por ejemplo).</summary>
public sealed record ChangeUserPasswordCommand(string Id, string NewPassword) : IRequest;
