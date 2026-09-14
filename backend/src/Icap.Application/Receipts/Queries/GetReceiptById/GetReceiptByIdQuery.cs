using Icap.Application.Receipts.DTOs;
using MediatR;

namespace Icap.Application.Receipts.Queries.GetReceiptById;

/// <summary>
/// Query de lectura para renderizar la vista/impresión de un recibo específico
/// (pantalla de Recibo en Angular, con QR y datos para las dos tarjetas).
/// </summary>
public sealed record GetReceiptByIdQuery(string ReceiptId) : IRequest<ReceiptDto>;
