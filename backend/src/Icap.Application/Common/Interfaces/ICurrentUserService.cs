using Icap.Domain.Enums;

namespace Icap.Application.Common.Interfaces;

/// <summary>
/// Abstracción sobre el usuario autenticado en la request HTTP actual
/// (extraído del JWT). La implementación concreta, basada en
/// HttpContext.User, vive en Presentation/Infrastructure para no acoplar
/// Application a ASP.NET Core.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
}
