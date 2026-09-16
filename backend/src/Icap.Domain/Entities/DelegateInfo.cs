using Icap.Domain.Common;
using Icap.Domain.Exceptions;
using Icap.Domain.ValueObjects;

namespace Icap.Domain.Entities;

/// <summary>
/// Sub-documento embebido en Receipt. Es una "fotografía" inmutable del
/// delegado al momento de emitir el recibo (histórico), por eso NO referencia
/// al User por navegación: si el nombre/área/correo del usuario cambia
/// después, el recibo ya emitido conserva el dato original — incluido el
/// Email, que es el destino real del envío del PDF (ver IEmailSender), no
/// necesariamente el mismo correo con el que el delegado inició sesión hoy.
///
/// Email es nullable a propósito: los recibos emitidos antes de que este
/// campo existiera no tienen esa información en Mongo, y forzar el dato al
/// rehidratarlos tronaría al listar/consultar recibos viejos. Application
/// exige el correo como obligatorio para recibos NUEVOS vía FluentValidation
/// (ver CreateReceiptCommandValidator); aquí solo se relaja para no romper
/// el histórico.
/// </summary>
public sealed class DelegateInfo : ValueObject
{
    public string DelegateName { get; }
    public string AreaOrRegion { get; }
    public Email? Email { get; }

    private DelegateInfo(string delegateName, string areaOrRegion, Email? email)
    {
        DelegateName = delegateName;
        AreaOrRegion = areaOrRegion;
        Email = email;
    }

    public static DelegateInfo Create(string delegateName, string areaOrRegion, string? email)
    {
        if (string.IsNullOrWhiteSpace(delegateName))
            throw new DomainException("El nombre del delegado (DelegateName) es requerido.");

        if (string.IsNullOrWhiteSpace(areaOrRegion))
            throw new DomainException("El área o región (AreaOrRegion) es requerida.");

        var emailVo = string.IsNullOrWhiteSpace(email) ? null : Email.Of(email);

        return new DelegateInfo(delegateName.Trim(), areaOrRegion.Trim(), emailVo);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DelegateName;
        yield return AreaOrRegion;
        yield return Email;
    }
}
