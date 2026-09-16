using Icap.Domain.Common;
using Icap.Domain.Entities;
using Icap.Domain.Enums;
using Icap.Domain.Exceptions;
using Icap.Domain.ValueObjects;

namespace Icap.Domain.Aggregates;

/// <summary>
/// Aggregate Root de la colección "Receipts". Modela el recibo temporal
/// emitido a un joven que compra pulseras para el evento ICAP.
/// Contiene sub-documentos embebidos (DelegateInfo, PurchaseDetails,
/// ReceiptValidation) para evitar joins al consultar, tal como especifica
/// el Documento de Diseño del Sistema.
/// </summary>
public sealed class Receipt : Entity<string>, IAggregateRoot
{
    public string FolioNumber { get; private set; } = default!;
    public DelegateInfo DelegateInfo { get; private set; } = default!;
    public PurchaseDetails PurchaseDetails { get; private set; } = default!;
    public ReceiptValidation Validation { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public string CreatedBy { get; private set; } = default!;

    // Constructor privado sin parámetros requerido por el mapeador de Infrastructure (BSON).
    private Receipt() { }

    private Receipt(
        string id,
        string folioNumber,
        DelegateInfo delegateInfo,
        PurchaseDetails purchaseDetails,
        ReceiptValidation validation,
        DateTime createdAt,
        string createdBy) : base(id)
    {
        FolioNumber = folioNumber;
        DelegateInfo = delegateInfo;
        PurchaseDetails = purchaseDetails;
        Validation = validation;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Factory method: emite un nuevo recibo. El folio y el QRHash se generan
    /// fuera del dominio (Infrastructure/Application) y se inyectan aquí ya
    /// resueltos, porque la generación secuencial/criptográfica es un detalle
    /// de infraestructura, no una regla de negocio del dominio.
    /// </summary>
    public static Receipt Issue(
        string id,
        string folioNumber,
        string delegateName,
        string areaOrRegion,
        string delegateEmail,
        int wristbandsQuantity,
        Money unitPrice,
        string qrHash,
        string createdBy)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new DomainException("El Id del recibo es requerido.");

        if (string.IsNullOrWhiteSpace(folioNumber))
            throw new DomainException("El número de folio (FolioNumber) es requerido.");

        if (string.IsNullOrWhiteSpace(createdBy))
            throw new DomainException("El recibo debe indicar quién lo generó (CreatedBy).");

        var delegateInfo = DelegateInfo.Create(delegateName, areaOrRegion, delegateEmail);
        var purchaseDetails = PurchaseDetails.Create(wristbandsQuantity, unitPrice);
        var validation = ReceiptValidation.Create(qrHash);

        return new Receipt(
            id,
            folioNumber,
            delegateInfo,
            purchaseDetails,
            validation,
            DateTime.UtcNow,
            createdBy);
    }

    /// <summary>
    /// Reconstruye un Receipt ya existente a partir de datos persistidos
    /// (MongoDB), preservando su estado real (incluido Status=Cancelled si
    /// aplica) sin reflection. Uso exclusivo de Infrastructure (mappers).
    /// </summary>
    public static Receipt Rehydrate(
        string id,
        string folioNumber,
        string delegateName,
        string areaOrRegion,
        string delegateEmail,
        int wristbandsQuantity,
        decimal unitPriceAmount,
        string currency,
        string qrHash,
        ReceiptStatus status,
        DateTime createdAt,
        string createdBy)
    {
        var unitPrice = Money.Of(unitPriceAmount, currency);
        var delegateInfo = DelegateInfo.Create(delegateName, areaOrRegion, delegateEmail);
        var purchaseDetails = PurchaseDetails.Create(wristbandsQuantity, unitPrice);
        var validation = ReceiptValidation.Create(qrHash);

        if (status == ReceiptStatus.Cancelled)
        {
            validation.Cancel();
        }

        return new Receipt(id, folioNumber, delegateInfo, purchaseDetails, validation, createdAt, createdBy);
    }

    /// <summary>Regla de negocio: solo un recibo Issued puede cancelarse.</summary>
    public void Cancel()
    {
        if (Validation.Status == ReceiptStatus.Cancelled)
            throw new DomainException($"El recibo con folio '{FolioNumber}' ya está cancelado.");

        Validation.Cancel();
    }

    public bool IsValidForRedemption() => Validation.Status == ReceiptStatus.Issued;
}
