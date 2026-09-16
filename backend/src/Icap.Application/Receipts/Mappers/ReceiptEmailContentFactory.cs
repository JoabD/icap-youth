using Icap.Domain.Aggregates;
using Icap.Domain.Services;

namespace Icap.Application.Receipts.Mappers;

/// <summary>
/// Construye el EmailMessage para un Receipt. Centralizado aquí porque tanto
/// el envío automático al emitir (CreateReceiptCommandHandler) como el
/// reenvío manual (SendReceiptEmailCommandHandler) arman exactamente el
/// mismo correo — duplicar la plantilla en dos Handlers sería el primer
/// lugar donde ambos textos se desincronizarían.
/// </summary>
internal static class ReceiptEmailContentFactory
{
    public static EmailMessage BuildReceiptEmail(Receipt receipt, byte[] pdfBytes)
    {
        var fileName = $"Recibo-{receipt.FolioNumber}.pdf";

        var htmlBody = $"""
            <p>Hola {receipt.DelegateInfo.DelegateName},</p>
            <p>Adjunto tu recibo <strong>{receipt.FolioNumber}</strong> por la compra de
            {receipt.PurchaseDetails.WristbandsQuantity} pulsera(s) — Pre venta Noviembre-Diciembre 2026.</p>
            <p>Total: <strong>${receipt.PurchaseDetails.TotalCost.Amount:N2} {receipt.PurchaseDetails.TotalCost.Currency}</strong></p>
            <p>Guarda este correo y el PDF adjunto; el código QR dentro del recibo es tu comprobante de validación.</p>
            <p>ICAP Juvenil - Sociedad Juvenil Amigos de Cristo</p>
            """;

        return new EmailMessage(
            ToEmail: receipt.DelegateInfo.Email!.Value,
            ToName: receipt.DelegateInfo.DelegateName,
            Subject: $"Tu recibo {receipt.FolioNumber} — ICAP Juvenil",
            HtmlBody: htmlBody,
            Attachments: new[] { new EmailAttachment(fileName, pdfBytes, "application/pdf") });
    }
}
