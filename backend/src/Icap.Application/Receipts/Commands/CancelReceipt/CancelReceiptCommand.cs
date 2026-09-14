using MediatR;

namespace Icap.Application.Receipts.Commands.CancelReceipt;

/// <summary>Command para anular un recibo emitido (ej. error de captura, devolución).</summary>
public sealed record CancelReceiptCommand(string ReceiptId) : IRequest;
