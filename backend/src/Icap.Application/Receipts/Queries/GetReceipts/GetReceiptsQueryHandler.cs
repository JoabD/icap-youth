using Icap.Application.Receipts.DTOs;
using Icap.Application.Receipts.Mappers;
using Icap.Domain.Repositories;
using MediatR;

namespace Icap.Application.Receipts.Queries.GetReceipts;

public sealed class GetReceiptsQueryHandler : IRequestHandler<GetReceiptsQuery, IReadOnlyList<ReceiptDto>>
{
    private readonly IReceiptRepository _receiptRepository;

    public GetReceiptsQueryHandler(IReceiptRepository receiptRepository)
    {
        _receiptRepository = receiptRepository;
    }

    public async Task<IReadOnlyList<ReceiptDto>> Handle(GetReceiptsQuery request, CancellationToken cancellationToken)
    {
        var receipts = string.IsNullOrWhiteSpace(request.CreatedByUserId)
            ? await _receiptRepository.GetAllAsync(cancellationToken)
            : await _receiptRepository.GetByCreatedByAsync(request.CreatedByUserId, cancellationToken);

        return receipts.Select(r => r.ToDto()).ToList();
    }
}
