using Icap.Application.Users.DTOs;
using Icap.Domain.Enums;
using MediatR;

namespace Icap.Application.Users.Commands.UpdateUser;

/// <summary>Edita el perfil (nombre, correo, rol) de un usuario existente. No toca la contraseña ni el estado activo/inactivo (ver ChangeUserPassword/Deactivate/Activate).</summary>
public sealed record UpdateUserCommand(
    string Id,
    string FullName,
    string Email,
    UserRole Role) : IRequest<UserDto>;
