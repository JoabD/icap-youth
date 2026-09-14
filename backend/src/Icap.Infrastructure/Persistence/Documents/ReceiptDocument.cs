using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Icap.Infrastructure.Persistence.Documents;

/// <summary>
/// Modelo de persistencia (colección "Receipts") con sub-documentos
/// embebidos, tal como especifica el Documento de Diseño del Sistema
/// (sin joins entre colecciones al consultar).
/// </summary>
[BsonIgnoreExtraElements]
public sealed class ReceiptDocument
{
    // Sin [BsonRepresentation(BsonType.ObjectId)] a propósito: el dominio no
    // garantiza que los Ids tengan forma de ObjectId (Application los genera
    // con Guid.NewGuid()), así que se guardan como string plano en _id.
    // MongoDB lo permite sin problema (el _id puede ser cualquier tipo BSON).
    [BsonId]
    public string Id { get; set; } = default!;

    [BsonElement("FolioNumber")]
    public string FolioNumber { get; set; } = default!;

    [BsonElement("DelegateInfo")]
    public DelegateInfoDocument DelegateInfo { get; set; } = default!;

    [BsonElement("PurchaseDetails")]
    public PurchaseDetailsDocument PurchaseDetails { get; set; } = default!;

    [BsonElement("Validation")]
    public ReceiptValidationDocument Validation { get; set; } = default!;

    [BsonElement("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    // Igual que Id: sin forzar representación ObjectId — es el Id de un
    // User, y tampoco se garantiza ese formato (ver comentario arriba).
    [BsonElement("CreatedBy")]
    public string CreatedBy { get; set; } = default!;
}

[BsonIgnoreExtraElements]
public sealed class DelegateInfoDocument
{
    [BsonElement("DelegateName")]
    public string DelegateName { get; set; } = default!;

    [BsonElement("AreaOrRegion")]
    public string AreaOrRegion { get; set; } = default!;
}

[BsonIgnoreExtraElements]
public sealed class PurchaseDetailsDocument
{
    [BsonElement("WristbandsQuantity")]
    public int WristbandsQuantity { get; set; }

    [BsonElement("UnitPriceAmount")]
    public decimal UnitPriceAmount { get; set; }

    [BsonElement("UnitPriceCurrency")]
    public string UnitPriceCurrency { get; set; } = default!;

    [BsonElement("TotalCostAmount")]
    public decimal TotalCostAmount { get; set; }

    [BsonElement("TotalCostCurrency")]
    public string TotalCostCurrency { get; set; } = default!;
}

[BsonIgnoreExtraElements]
public sealed class ReceiptValidationDocument
{
    [BsonElement("QRHash")]
    public string QRHash { get; set; } = default!;

    /// <summary>Se persiste como string ("Issued"/"Cancelled") para legibilidad directa en Mongo.</summary>
    [BsonElement("Status")]
    [BsonRepresentation(BsonType.String)]
    public string Status { get; set; } = default!;
}
