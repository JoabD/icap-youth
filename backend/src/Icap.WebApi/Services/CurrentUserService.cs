using System.Security.Claims;
using Icap.Application.Common.Interfaces;
using Icap.Domain.Enums;

namespace Icap.WebApi.Services;

/// <summary>
/// Implementación de ICurrentUserService (declarada en Application) basada
/// en el HttpContext.User que puebla el middleware de autenticación JWT.
/// Es el único punto de Icap.WebApi que Application "ve" indirectamente,
/// vía la interfaz — Application nunca referencia ASP.NET Core directamente.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? User?.FindFirstValue("sub");

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public UserRole? Role
    {
        get
        {
            var roleClaim = User?.FindFirstValue(ClaimTypes.Role);
            return roleClaim is not null && Enum.TryParse<UserRole>(roleClaim, out var role) ? role : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
}
