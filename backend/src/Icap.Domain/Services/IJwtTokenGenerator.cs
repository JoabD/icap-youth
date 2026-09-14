using Icap.Domain.Aggregates;

namespace Icap.Domain.Services;

/// <summary>
/// Contrato para la generación de tokens JWT tras un login exitoso.
/// La implementación (firma, claims, expiración) vive en Infrastructure.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>Genera el token de acceso para el usuario autenticado.</summary>
    string GenerateToken(User user);
}
