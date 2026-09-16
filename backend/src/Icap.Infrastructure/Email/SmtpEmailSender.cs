using Icap.Domain.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Icap.Infrastructure.Email;

/// <summary>
/// Implementación de IEmailSender vía SMTP (Gmail con App Password por
/// default — gratis, hasta ~500 destinatarios/día, suficiente para el
/// volumen de un evento juvenil; si el volumen crece, migrar a un proveedor
/// transaccional como Brevo/Resend solo implica cambiar esta clase, no
/// Application ni Domain).
/// </summary>
public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;

    public SmtpEmailSender(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_settings.SenderDisplayName, _settings.SenderEmail));
        mimeMessage.To.Add(new MailboxAddress(message.ToName, message.ToEmail));

        // El Cc administrativo se agrega aquí (Infrastructure), no en Application,
        // porque es una decisión de configuración de este despliegue (ver
        // EmailSettings.DefaultCcAddress), no una regla de negocio del dominio.
        if (!string.IsNullOrWhiteSpace(_settings.DefaultCcAddress))
        {
            mimeMessage.Cc.Add(MailboxAddress.Parse(_settings.DefaultCcAddress));
        }

        mimeMessage.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = message.HtmlBody };

        foreach (var attachment in message.Attachments)
        {
            bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
        }

        mimeMessage.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.SmtpHost, _settings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(_settings.SenderEmail, _settings.SenderAppPassword, cancellationToken);
        await client.SendAsync(mimeMessage, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
