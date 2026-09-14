namespace Icap.Domain.Services;

/// <summary>
/// Contrato para el servicio de hashing de contraseñas. El dominio declara
/// la interfaz porque necesita este comportamiento en sus reglas (User),
/// pero el algoritmo concreto (ej. BCrypt/Argon2) es un detalle técnico
/// que se implementa en Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string plainPassword);

    bool Verify(string plainPassword, string passwordHash);
}
