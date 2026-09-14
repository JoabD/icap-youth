using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Icap.Infrastructure.Persistence.Documents;

/// <summary>
/// Modelo de persistencia (colección "Users") separado del Aggregate del
/// dominio a propósito: así Icap.Domain jamás referencia MongoDB.Driver, y
/// cualquier cambio de esquema/serialización se resuelve solo aquí, en
/// Infrastructure. El mapeo Document ⇄ Aggregate vive en UserMapper.
/// </summary>
[BsonIgnoreExtraElements]
public sealed class UserDocument
{
    // Sin [BsonRepresentation(BsonType.ObjectId)] a propósito: el dominio no
    // garantiza que los Ids tengan forma de ObjectId. Hoy DbSeeder sí genera
    // Ids con ObjectId.GenerateNewId(), pero forzar la conversión aquí haría
    // que un futuro alta de usuario con otro esquema de Id (p. ej. Guid,
    // como ya ocurre en CreateReceiptCommandHandler) reviente al insertar.
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("FullName")]
    public string FullName { get; set; } = default!;

    [BsonElement("Email")]
    public string Email { get; set; } = default!;

    [BsonElement("PasswordHash")]
    public string PasswordHash { get; set; } = default!;

    /// <summary>Se persiste como string ("Admin"/"Delegate") para legibilidad directa en Mongo.</summary>
    [BsonElement("Role")]
    [BsonRepresentation(BsonType.String)]
    public string Role { get; set; } = default!;

    [BsonElement("IsActive")]
    public bool IsActive { get; set; }

    [BsonElement("ExtraAttributes")]
    public Dictionary<string, object> ExtraAttributes { get; set; } = new();
}
