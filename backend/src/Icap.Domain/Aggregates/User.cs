using Icap.Domain.Common;
using Icap.Domain.Enums;
using Icap.Domain.Exceptions;
using Icap.Domain.ValueObjects;

namespace Icap.Domain.Aggregates;

/// <summary>
/// Aggregate Root de la colección "Users". Representa tanto a administradores
/// como a delegados que pueden autenticarse en la plataforma.
/// El Id se representa como string en el dominio (mapea a ObjectId en Mongo,
/// detalle que resuelve Infrastructure para no acoplar el dominio al driver).
/// </summary>
public sealed class User : Entity<string>, IAggregateRoot
{
    public string FullName { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>Datos dinámicos futuros sin necesidad de migrar el esquema.</summary>
    public IDictionary<string, object> ExtraAttributes { get; private set; } = new Dictionary<string, object>();

    // Constructor privado sin parámetros requerido por el mapeador de Infrastructure (BSON).
    private User() { }

    private User(string id, string fullName, Email email, string passwordHash, UserRole role)
        : base(id)
    {
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
    }

    /// <summary>
    /// Factory method para crear un nuevo usuario. El hash de la contraseña
    /// debe generarse en Infrastructure (IPasswordHasher) antes de invocar esto,
    /// ya que el algoritmo de hashing es un detalle de infraestructura.
    /// </summary>
    public static User Create(string id, string fullName, string email, string passwordHash, UserRole role)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("El Id del usuario es requerido.");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("El nombre completo (FullName) es requerido.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("El PasswordHash es requerido.");

        return new User(id, fullName.Trim(), Email.Of(email), passwordHash, role);
    }

    /// <summary>
    /// Reconstruye un User ya existente a partir de datos persistidos (MongoDB),
    /// preservando su estado real (IsActive, ExtraAttributes) sin reflection.
    /// Uso exclusivo de Infrastructure (mappers de repositorio); a diferencia de
    /// <see cref="Create"/>, no asume valores por defecto de un alta nueva.
    /// </summary>
    public static User Rehydrate(
        string id,
        string fullName,
        string email,
        string passwordHash,
        UserRole role,
        bool isActive,
        IDictionary<string, object>? extraAttributes = null)
    {
        var user = new User(id, fullName, Email.Of(email), passwordHash, role);

        if (!isActive)
        {
            user.IsActive = false;
        }

        if (extraAttributes is not null)
        {
            user.ExtraAttributes = new Dictionary<string, object>(extraAttributes);
        }

        return user;
    }

    /// <summary>
    /// Actualiza los datos editables de perfil (todo lo demás — contraseña,
    /// rol, estado activo/inactivo — tiene su propio método dedicado con su
    /// propia regla de negocio, para no mezclar invariantes distintas aquí).
    /// </summary>
    public void UpdateProfile(string fullName, string email)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new DomainException("El nombre completo (FullName) es requerido.");

        FullName = fullName.Trim();
        Email = Email.Of(email);
    }

    public void ChangeRole(UserRole role) => Role = role;

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("El usuario ya se encuentra inactivo.");

        IsActive = false;
    }

    public void Activate() => IsActive = true;

    public void ChangePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException("El nuevo PasswordHash no puede estar vacío.");

        PasswordHash = newPasswordHash;
    }

    public void SetExtraAttribute(string key, object value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new DomainException("La llave del atributo extra es requerida.");

        ExtraAttributes[key] = value;
    }

    /// <summary>Regla de negocio: solo usuarios activos pueden iniciar sesión.</summary>
    public void EnsureCanAuthenticate()
    {
        if (!IsActive)
            throw new DomainException("El usuario está inactivo y no puede iniciar sesión.");
    }
}
