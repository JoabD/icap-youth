using Icap.Domain.Aggregates;

namespace Icap.Domain.Services;

/// <summary>
/// Contrato para generar la representación en PDF (estilo factura) de un
/// recibo ya emitido. La librería concreta (QuestPDF) es un detalle de
/// Infrastructure; el dominio solo expone "puedo convertir este Receipt en
/// un PDF", igual que ya hace con IQrHashGenerator/IJwtTokenGenerator.
/// </summary>
public interface IReceiptPdfGenerator
{
    byte[] Generate(Receipt receipt);
}
