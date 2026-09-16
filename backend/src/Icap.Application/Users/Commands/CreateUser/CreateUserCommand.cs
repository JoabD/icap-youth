using Icap.Application.Users.DTOs;
using Icap.Domain.Enums;
using MediatR;

namespace Icap.Application.Users.Commands.CreateUser;

/// <summary>Da de alta un nuevo usuario (Admin o Delegate). Reemplaza al seed manual para altas posteriores al arranque inicial.</summary>
public sealed record CreateUserCommand(
    string FullName,
    string Email,
    string Password,
    UserRole Role) : IRequest<UserDto>;
