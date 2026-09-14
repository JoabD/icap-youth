using Icap.Domain.Aggregates;
using Icap.Domain.Enums;
using Icap.Infrastructure.Persistence.Documents;

namespace Icap.Infrastructure.Persistence.Mappers;

internal static class ReceiptMapper
{
    public static Receipt ToDomain(this ReceiptDocument document) => Receipt.Rehydrate(
        id: document.Id,
        folioNumber: document.FolioNumber,
        delegateName: document.DelegateInfo.DelegateName,
        areaOrRegion: document.DelegateInfo.AreaOrRegion,
        wristbandsQuantity: document.PurchaseDetails.WristbandsQuantity,
        unitPriceAmount: document.PurchaseDetails.UnitPriceAmount,
        currency: document.PurchaseDetails.UnitPriceCurrency,
        qrHash: document.Validation.QRHash,
        status: Enum.Parse<ReceiptStatus>(document.Validation.Status),
        createdAt: document.CreatedAt,
        createdBy: document.CreatedBy);

    public static ReceiptDocument ToDocument(this Receipt receipt) => new()
    {
        Id = receipt.Id,
        FolioNumber = receipt.FolioNumber,
        DelegateInfo = new DelegateInfoDocument
        {
            DelegateName = receipt.DelegateInfo.DelegateName,
            AreaOrRegion = receipt.DelegateInfo.AreaOrRegion,
        },
        PurchaseDetails = new PurchaseDetailsDocument
        {
            WristbandsQuantity = receipt.PurchaseDetails.WristbandsQuantity,
            UnitPriceAmount = receipt.PurchaseDetails.UnitPrice.Amount,
            UnitPriceCurrency = receipt.PurchaseDetails.UnitPrice.Currency,
            TotalCostAmount = receipt.PurchaseDetails.TotalCost.Amount,
            TotalCostCurrency = receipt.PurchaseDetails.TotalCost.Currency,
        },
        Validation = new ReceiptValidationDocument
        {
            QRHash = receipt.Validation.QRHash,
            Status = receipt.Validation.Status.ToString(),
        },
        CreatedAt = receipt.CreatedAt,
        CreatedBy = receipt.CreatedBy,
    };
}
