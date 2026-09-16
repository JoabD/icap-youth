using System.Globalization;
using System.Reflection;
using Icap.Domain.Aggregates;
using Icap.Domain.Services;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Icap.Infrastructure.Pdf;

/// <summary>
/// Genera el recibo en PDF con formato de factura, usando QuestPDF.
///
/// Licencia: QuestPDF Community License — gratuita para organizaciones sin
/// fines de lucro (como ICAP Juvenil) y para negocios con ingresos anuales
/// menores a USD 1,000,000. Ver https://www.questpdf.com/license/community.html.
/// Si este proyecto alguna vez deja de calificar, hay que migrar a la
/// licencia Professional/Enterprise antes de seguir generando PDFs.
/// </summary>
public sealed class QuestPdfReceiptGenerator : IReceiptPdfGenerator
{
    private const string CertifierName = "ICAP Juvenil - Sociedad Juvenil Amigos de Cristo";
    private const string Concept = "Pre venta pulseras — Noviembre-Diciembre 2026";
    private static readonly CultureInfo Culture = new("es-MX");
    private static readonly byte[] LogoBytes = LoadEmbeddedResource("logo-sociedad-juvenil.png");

    // Firma pre-autorizada de ICAP Juvenil: es un sello visual para que el
    // recibo salga "ya firmado" por la certificadora sin trabajo manual, no
    // una firma digital/criptográfica. La validez real del documento sigue
    // dependiendo del QR + QRHash firmado con HMAC (Validation.QRHash), que
    // es lo que efectivamente se verifica al canjear.
    private static readonly byte[] SignatureBytes = LoadEmbeddedResource("signature-icap.png");

    static QuestPdfReceiptGenerator()
    {
        // Declaración de licencia requerida por QuestPDF desde v2023+; sin esto,
        // la librería lanza una excepción al primer Generate(). Ver comentario
        // de licencia arriba: esta app SÍ califica para Community.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(Receipt receipt)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontFamily("Helvetica").FontSize(10));

                page.Header().Element(header => ComposeHeader(header, receipt));
                page.Content().Element(content => ComposeContent(content, receipt));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Este documento certifica la compra de pulseras del evento. Preséntalo (impreso o digital) junto con el código QR para su validación.")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(QuestPDF.Infrastructure.IContainer container, Receipt receipt)
    {
        container.Row(row =>
        {
            row.ConstantItem(70).Height(70).Image(LogoBytes).FitArea();

            row.RelativeItem().PaddingLeft(10).Column(column =>
            {
                column.Item().Text(CertifierName).FontSize(14).Bold();
                column.Item().Text(Concept).FontSize(10).FontColor(Colors.Grey.Darken2);
            });

            row.ConstantItem(160).Column(column =>
            {
                column.Item().AlignRight().Text($"Recibo {receipt.FolioNumber}").FontSize(13).Bold();
                column.Item().AlignRight().Text(receipt.CreatedAt.ToString("dd 'de' MMMM 'de' yyyy, HH:mm", Culture)).FontSize(9);
                column.Item().AlignRight().Text(receipt.Validation.Status == Domain.Enums.ReceiptStatus.Cancelled ? "CANCELADO" : "VÁLIDO")
                    .FontSize(10).Bold()
                    .FontColor(receipt.Validation.Status == Domain.Enums.ReceiptStatus.Cancelled ? Colors.Red.Medium : Colors.Green.Darken1);
            });
        });
    }

    private static void ComposeContent(QuestPDF.Infrastructure.IContainer container, Receipt receipt)
    {
        container.PaddingTop(20).Column(column =>
        {
            column.Spacing(15);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Delegado").FontSize(9).FontColor(Colors.Grey.Darken1);
                    c.Item().Text(receipt.DelegateInfo.DelegateName).FontSize(11).Bold();
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Área / Región").FontSize(9).FontColor(Colors.Grey.Darken1);
                    c.Item().Text(receipt.DelegateInfo.AreaOrRegion).FontSize(11).Bold();
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Correo").FontSize(9).FontColor(Colors.Grey.Darken1);
                    c.Item().Text(receipt.DelegateInfo.Email?.Value ?? "—").FontSize(11).Bold();
                });
            });

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4);
                    columns.RelativeColumn(1);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).Text("Concepto");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Cant.");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Precio unitario");
                    header.Cell().Element(HeaderCell).AlignRight().Text("Importe");

                    static QuestPDF.Infrastructure.IContainer HeaderCell(QuestPDF.Infrastructure.IContainer c) =>
                        c.DefaultTextStyle(x => x.SemiBold().FontColor(Colors.White))
                         .Background(Colors.Grey.Darken3)
                         .PaddingVertical(6).PaddingHorizontal(4);
                });

                table.Cell().Element(BodyCell).Text(Concept);
                table.Cell().Element(BodyCell).AlignRight().Text(receipt.PurchaseDetails.WristbandsQuantity.ToString());
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(receipt.PurchaseDetails.UnitPrice.Amount, receipt.PurchaseDetails.UnitPrice.Currency));
                table.Cell().Element(BodyCell).AlignRight().Text(FormatMoney(receipt.PurchaseDetails.TotalCost.Amount, receipt.PurchaseDetails.TotalCost.Currency));

                static QuestPDF.Infrastructure.IContainer BodyCell(QuestPDF.Infrastructure.IContainer c) =>
                    c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(8).PaddingHorizontal(4);
            });

            column.Item().AlignRight().PaddingTop(5).Column(c =>
            {
                c.Item().Text($"Total: {FormatMoney(receipt.PurchaseDetails.TotalCost.Amount, receipt.PurchaseDetails.TotalCost.Currency)}")
                    .FontSize(14).Bold();
            });

            column.Item().PaddingTop(15).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Firma del delegado").FontSize(9).FontColor(Colors.Grey.Darken1);
                    c.Item().PaddingTop(30).BorderTop(1).BorderColor(Colors.Grey.Lighten1);
                });

                row.ConstantItem(20);

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Sello / Autorización ICAP").FontSize(9).FontColor(Colors.Grey.Darken1);
                    c.Item().AlignCenter().Height(30).Image(SignatureBytes).FitArea();
                    c.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten1);
                });

                row.ConstantItem(110).Column(c =>
                {
                    c.Item().AlignCenter().Text("Código de validación").FontSize(8).FontColor(Colors.Grey.Darken1);
                    c.Item().AlignCenter().Height(90).Width(90).Svg(BuildQrSvg(receipt.Validation.QRHash));
                });
            });
        });
    }

    private static string FormatMoney(decimal amount, string currency) =>
        $"${amount.ToString("N2", Culture)} {currency}";

    private static string BuildQrSvg(string qrHash)
    {
        // QRCoder (MIT) genera el SVG del QR a partir del mismo QRHash que ya
        // se firma con HMAC-SHA256 en QrHashGenerator — es el mismo valor que
        // el frontend pinta con angularx-qrcode, para que ambos códigos QR
        // (pantalla y PDF) sean idénticos y validen contra el mismo hash.
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrHash, QRCodeGenerator.ECCLevel.Q);
        var svgQrCode = new SvgQRCode(qrCodeData);
        return svgQrCode.GetGraphic(20);
    }

    private static byte[] LoadEmbeddedResource(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException(
                $"No se encontró el recurso embebido '{fileName}' en Icap.Infrastructure. " +
                "Verifica que el archivo exista en Assets/ y esté declarado como EmbeddedResource en el .csproj.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}
