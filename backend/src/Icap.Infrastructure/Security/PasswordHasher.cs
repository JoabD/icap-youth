using Icap.Domain.Services;

namespace Icap.Infrastructure.Security;

/// <summary>
/// Implementación de IPasswordHasher usando BCrypt (work factor 12), el
/// algoritmo estándar de facto para hashing de contraseñas en .NET.
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string plainPassword) =>
        BCrypt.Net.BCrypt.HashPassword(plainPassword, workFactor: WorkFactor);

    public bool Verify(string plainPassword, string passwordHash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(plainPassword, passwordHash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash corrupto/formato inválido: se trata como credencial inválida, no como excepción 500.
            return false;
        }
    }
}
