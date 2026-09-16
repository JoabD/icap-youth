namespace Icap.Application.Users.DTOs;

/// <summary>DTO de lectura para un User, aplanado para el frontend (tabla de administración). Nunca incluye PasswordHash.</summary>
public sealed record UserDto(
    string Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive);
