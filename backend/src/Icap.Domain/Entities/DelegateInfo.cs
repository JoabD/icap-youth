using Icap.Domain.Common;
using Icap.Domain.Exceptions;

namespace Icap.Domain.Entities;

/// <summary>
/// Sub-documento embebido en Receipt. Es una "fotografía" inmutable del
/// delegado al momento de emitir el recibo (histórico), por eso NO referencia
/// al User por navegación: si el nombre/área del usuario cambia después,
/// el recibo ya emitido conserva el dato original.
/// </summary>
public sealed class DelegateInfo : ValueObject
{
    public string DelegateName { get; }
    public string AreaOrRegion { get; }

    private DelegateInfo(string delegateName, string areaOrRegion)
    {
        DelegateName = delegateName;
        AreaOrRegion = areaOrRegion;
    }

    public static DelegateInfo Create(string delegateName, string areaOrRegion)
    {
        if (string.IsNullOrWhiteSpace(delegateName))
            throw new DomainException("El nombre del delegado (DelegateName) es requerido.");

        if (string.IsNullOrWhiteSpace(areaOrRegion))
            throw new DomainException("El área o región (AreaOrRegion) es requerida.");

        return new DelegateInfo(delegateName.Trim(), areaOrRegion.Trim());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DelegateName;
        yield return AreaOrRegion;
    }
}
