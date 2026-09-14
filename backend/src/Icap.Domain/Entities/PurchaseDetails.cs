using Icap.Domain.Common;
using Icap.Domain.Exceptions;
using Icap.Domain.ValueObjects;

namespace Icap.Domain.Entities;

/// <summary>
/// Sub-documento embebido en Receipt con el detalle de la compra de pulseras.
/// TotalCost se calcula en el dominio (UnitPrice * WristbandsQuantity) para
/// que la regla de negocio viva en un solo lugar, no en el frontend ni en la API.
/// </summary>
public sealed class PurchaseDetails : ValueObject
{
    public int WristbandsQuantity { get; }
    public Money UnitPrice { get; }
    public Money TotalCost { get; }

    private PurchaseDetails(int wristbandsQuantity, Money unitPrice, Money totalCost)
    {
        WristbandsQuantity = wristbandsQuantity;
        UnitPrice = unitPrice;
        TotalCost = totalCost;
    }

    public static PurchaseDetails Create(int wristbandsQuantity, Money unitPrice)
    {
        if (wristbandsQuantity <= 0)
            throw new DomainException("La cantidad de pulseras (WristbandsQuantity) debe ser mayor a cero.");

        var totalCost = unitPrice.Multiply(wristbandsQuantity);

        return new PurchaseDetails(wristbandsQuantity, unitPrice, totalCost);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return WristbandsQuantity;
        yield return UnitPrice;
        yield return TotalCost;
    }
}
