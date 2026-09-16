using MediatR;

namespace Icap.Application.Receipts.Commands.SendReceiptEmail;

/// <summary>Envía (o reenvía) el PDF del recibo al correo del delegado, con copia administrativa (ver EmailSettings.DefaultCcAddress en Infrastructure).</summary>
public sealed record SendReceiptEmailCommand(string Id) : IRequest;
