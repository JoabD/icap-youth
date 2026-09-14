using Icap.Application.Common.Exceptions;
using Icap.Application.Receipts.DTOs;
using Icap.Application.Receipts.Mappers;
using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using MediatR;

namespace Icap.Application.Receipts.Queries.GetReceiptById;

public sealed class GetReceiptByIdQueryHandler : IRequestHandler<GetReceiptByIdQuery, ReceiptDto>
{
    private readonly IReceiptRepository _receiptRepository;

    public GetReceiptByIdQueryHandler(IReceiptRepository receiptRepository)
    {
        _receiptRepository = receiptRepository;
    }

    public async Task<ReceiptDto> Handle(GetReceiptByIdQuery request, CancellationToken cancellationToken)
    {
        var receipt = await _receiptRepository.GetByIdAsync(request.ReceiptId, cancellationToken)
            ?? throw new NotFoundException(nameof(Receipt), request.ReceiptId);

        return receipt.ToDto();
    }
}
