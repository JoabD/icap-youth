using Icap.Domain.Aggregates;
using Icap.Domain.Enums;
using Icap.Domain.Services;
using Icap.Infrastructure.Persistence.Documents;
using Icap.Infrastructure.Persistence.Mappers;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Icap.Infrastructure.Persistence;

/// <summary>
/// Seed idempotente de datos iniciales: si la colección "Users" ya tiene al
/// menos un documento, no hace nada. Pensado para correr en cada arranque
/// (Program.cs) sin duplicar usuarios ni requerir un paso manual aparte.
///
/// Credenciales de ejemplo (SOLO para desarrollo/demo — cámbialas o borra
/// este seed antes de ir a producción):
///   Admin:       admin@icapjuvenil.org    / Admin123!
///   Delegado:    delegado@icapjuvenil.org / Delegado123!
///   Super Admin: joabdgtz@gmail.com       / IcapAdmin2026! (cámbiala de inmediato con
///                POST /api/v1/users/{id}/password una vez que inicies sesión)
/// </summary>
public static class DbSeeder
{
    public const string SeedAdminEmail = "admin@icapjuvenil.org";
    public const string SeedAdminPassword = "Admin123!";
    public const string SeedDelegateEmail = "delegado@icapjuvenil.org";
    public const string SeedDelegatePassword = "Delegado123!";
    public const string SeedSuperAdminEmail = "joabdgtz@gmail.com";
    public const string SeedSuperAdminPassword = "IcapAdmin2026!";

    public static async Task SeedAsync(
        IMongoDbContext context,
        IPasswordHasher passwordHasher,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var alreadySeeded = await context.Users
            .Find(FilterDefinition<UserDocument>.Empty)
            .AnyAsync(cancellationToken);

        if (alreadySeeded)
        {
            return;
        }

        var admin = User.Create(
            id: ObjectId.GenerateNewId().ToString(),
            fullName: "Administrador ICAP",
            email: SeedAdminEmail,
            passwordHash: passwordHasher.Hash(SeedAdminPassword),
            role: UserRole.Admin);

        var demoDelegate = User.Create(
            id: ObjectId.GenerateNewId().ToString(),
            fullName: "Delegado Demo",
            email: SeedDelegateEmail,
            passwordHash: passwordHasher.Hash(SeedDelegatePassword),
            role: UserRole.Delegate);

        var superAdmin = User.Create(
            id: ObjectId.GenerateNewId().ToString(),
            fullName: "Joab David Gutierrez",
            email: SeedSuperAdminEmail,
            passwordHash: passwordHasher.Hash(SeedSuperAdminPassword),
            role: UserRole.Admin);

        await context.Users.InsertManyAsync(
            new[] { admin.ToDocument(), demoDelegate.ToDocument(), superAdmin.ToDocument() },
            cancellationToken: cancellationToken);

        logger.LogWarning(
            "Seed inicial aplicado: se crearon los usuarios de ejemplo '{AdminEmail}', '{DelegateEmail}' y '{SuperAdminEmail}'. " +
            "Cambia sus contraseñas o elimina el seed antes de producción.",
            SeedAdminEmail,
            SeedDelegateEmail,
            SeedSuperAdminEmail);
    }
}
