using Icap.Application.Receipts.DTOs;
using MediatR;

namespace Icap.Application.Receipts.Queries.GetReceiptPdf;

public sealed record GetReceiptPdfQuery(string Id) : IRequest<ReceiptPdfDto>;
