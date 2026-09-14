namespace Icap.Application.Auth.DTOs;

/// <summary>DTO de respuesta tras un login exitoso: token JWT + datos básicos del usuario.</summary>
public sealed record LoginResultDto(
    string AccessToken,
    string UserId,
    string FullName,
    string Email,
    string Role);
