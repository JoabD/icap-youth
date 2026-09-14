using Icap.Application.Receipts.DTOs;
using MediatR;

namespace Icap.Application.Receipts.Queries.GetReceipts;

/// <summary>
/// Query de listado para la tabla de registro de recibos (Angular Material Table).
/// CreatedByUserId es opcional: un Delegate solo ve los suyos, un Admin ve todos
/// (esa decisión de autorización se resuelve en el Handler según ICurrentUserService).
/// </summary>
public sealed record GetReceiptsQuery(string? CreatedByUserId = null) : IRequest<IReadOnlyList<ReceiptDto>>;
