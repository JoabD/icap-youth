namespace Icap.Application.Receipts.DTOs;

/// <summary>DTO de lectura para un Receipt, aplanado para el frontend (tabla/recibo imprimible).</summary>
public sealed record ReceiptDto(
    string Id,
    string FolioNumber,
    string DelegateName,
    string AreaOrRegion,
    int WristbandsQuantity,
    decimal UnitPrice,
    decimal TotalCost,
    string Currency,
    string QRHash,
    string Status,
    DateTime CreatedAt,
    string CreatedBy);
