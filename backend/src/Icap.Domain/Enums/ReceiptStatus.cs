namespace Icap.Domain.Enums;

/// <summary>
/// Estado de validación de un recibo (Receipt.Validation.Status en MongoDB).
/// Issued: recibo vigente y válido para canje de pulsera.
/// Cancelled: recibo anulado (ej. error de captura o devolución).
/// </summary>
public enum ReceiptStatus
{
    Issued = 0,
    Cancelled = 1
}
