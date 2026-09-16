using Icap.Application.Receipts.DTOs;
using Icap.Domain.Aggregates;

namespace Icap.Application.Receipts.Mappers;

/// <summary>
/// Mapeo Aggregate -> DTO. Se centraliza aquí (en lugar de repetirlo en cada
/// Handler) para que Create/Cancel/GetById/GetAll devuelvan siempre la misma forma.
/// </summary>
public static class ReceiptDtoMapper
{
    public static ReceiptDto ToDto(this Receipt receipt) => new(
        Id: receipt.Id,
        FolioNumber: receipt.FolioNumber,
        DelegateName: receipt.DelegateInfo.DelegateName,
        AreaOrRegion: receipt.DelegateInfo.AreaOrRegion,
        DelegateEmail: receipt.DelegateInfo.Email?.Value,
        WristbandsQuantity: receipt.PurchaseDetails.WristbandsQuantity,
        UnitPrice: receipt.PurchaseDetails.UnitPrice.Amount,
        TotalCost: receipt.PurchaseDetails.TotalCost.Amount,
        Currency: receipt.PurchaseDetails.TotalCost.Currency,
        QRHash: receipt.Validation.QRHash,
        Status: receipt.Validation.Status.ToString(),
        CreatedAt: receipt.CreatedAt,
        CreatedBy: receipt.CreatedBy);
}
