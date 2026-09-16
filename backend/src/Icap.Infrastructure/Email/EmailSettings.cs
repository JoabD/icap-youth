namespace Icap.Infrastructure.Email;

/// <summary>Se enlaza a la sección "EmailSettings" de appsettings.json.</summary>
public sealed class EmailSettings
{
    public const string SectionName = "EmailSettings";

    public string SmtpHost { get; set; } = "smtp.gmail.com";

    public int SmtpPort { get; set; } = 587;

    /// <summary>Cuenta de Gmail que envía los correos (ej. recibos@icapjuvenil.org o una cuenta personal).</summary>
    public string SenderEmail { get; set; } = default!;

    /// <summary>
    /// App Password de Gmail (16 caracteres, generado en myaccount.google.com/apppasswords
    /// con la verificación en 2 pasos activada) — NUNCA la contraseña normal de la cuenta.
    /// </summary>
    public string SenderAppPassword { get; set; } = default!;

    public string SenderDisplayName { get; set; } = "ICAP Juvenil";

    /// <summary>Correo que siempre recibe copia (Cc) de cada recibo enviado, sin que Application tenga que conocerlo.</summary>
    public string? DefaultCcAddress { get; set; }
}
