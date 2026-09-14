namespace Icap.Domain.Services;

/// <summary>
/// Contrato para generar el valor QRHash de un recibo: la cadena
/// (encriptada o firmada) que el frontend usará para pintar el QR
/// físico en el recibo impreso. La implementación (ej. HMAC sobre
/// folio+fecha, o un GUID firmado) vive en Infrastructure.
/// </summary>
public interface IQrHashGenerator
{
    string Generate(string folioNumber, string receiptId);
}
