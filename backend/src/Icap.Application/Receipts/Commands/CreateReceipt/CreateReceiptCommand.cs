using Icap.Application.Receipts.DTOs;
using MediatR;

namespace Icap.Application.Receipts.Commands.CreateReceipt;

/// <summary>
/// Command para emitir un nuevo recibo temporal de compra de pulseras.
/// DelegateName/AreaOrRegion/CreatedBy provienen del usuario autenticado
/// (ver LoginResultDto/ICurrentUserService), no de input libre del cliente,
/// para preservar la integridad del histórico.
/// </summary>
public sealed record CreateReceiptCommand(
    string DelegateName,
    string AreaOrRegion,
    string DelegateEmail,
    int WristbandsQuantity,
    decimal UnitPrice,
    string CreatedByUserId) : IRequest<ReceiptDto>;
