using Icap.Domain.Common;
using Icap.Domain.Enums;
using Icap.Domain.Exceptions;

namespace Icap.Domain.Entities;

/// <summary>
/// Sub-documento embebido en Receipt: información de validación/QR.
/// QRHash es el valor que el frontend usa para renderizar el código QR
/// (con angularx-qrcode); su contenido concreto lo decide el servicio
/// de encriptación de Infrastructure (interfaz definida en el dominio).
/// </summary>
public sealed class ReceiptValidation : ValueObject
{
    public string QRHash { get; private set; }
    public ReceiptStatus Status { get; private set; }

    private ReceiptValidation(string qrHash, ReceiptStatus status)
    {
        QRHash = qrHash;
        Status = status;
    }

    public static ReceiptValidation Create(string qrHash)
    {
        if (string.IsNullOrWhiteSpace(qrHash))
            throw new DomainException("El QRHash es requerido para validar el recibo.");

        return new ReceiptValidation(qrHash, ReceiptStatus.Issued);
    }

    public void Cancel()
    {
        if (Status == ReceiptStatus.Cancelled)
            throw new DomainException("El recibo ya se encuentra cancelado.");

        Status = ReceiptStatus.Cancelled;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return QRHash;
        yield return Status;
    }
}
