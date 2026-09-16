namespace Icap.Domain.Services;

/// <summary>
/// Contrato genérico de envío de correo (adjuntos incluidos). El proveedor
/// concreto (SMTP de Gmail vía MailKit, hoy) es un detalle de Infrastructure;
/// cualquier copia oculta/administrativa que deba ir siempre en Cc también
/// es responsabilidad de Infrastructure (ver EmailSettings), no del dominio
/// ni de Application, para no esparcir esa regla de configuración en varios
/// lugares.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public sealed record EmailMessage(
    string ToEmail,
    string ToName,
    string Subject,
    string HtmlBody,
    IReadOnlyList<EmailAttachment> Attachments);

public sealed record EmailAttachment(string FileName, byte[] Content, string ContentType);
