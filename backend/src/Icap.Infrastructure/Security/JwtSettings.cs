namespace Icap.Infrastructure.Security;

/// <summary>Se enlaza a la sección "JwtSettings" de appsettings.json.</summary>
public sealed class JwtSettings
{
    public const string SectionName = "JwtSettings";

    /// <summary>Clave simétrica de firma. En producción debe venir de un secret manager, no de appsettings.json en claro.</summary>
    public string Secret { get; set; } = default!;

    public string Issuer { get; set; } = default!;

    public string Audience { get; set; } = default!;

    public int ExpirationMinutes { get; set; } = 480;
}
