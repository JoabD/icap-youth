using Icap.Application.Common.Exceptions;
using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using MediatR;

namespace Icap.Application.Receipts.Commands.CancelReceipt;

public sealed class CancelReceiptCommandHandler : IRequestHandler<CancelReceiptCommand>
{
    private readonly IReceiptRepository _receiptRepository;

    public CancelReceiptCommandHandler(IReceiptRepository receiptRepository)
    {
        _receiptRepository = receiptRepository;
    }

    public async Task Handle(CancelReceiptCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _receiptRepository.GetByIdAsync(request.ReceiptId, cancellationToken)
            ?? throw new NotFoundException(nameof(Receipt), request.ReceiptId);

        // Regla de negocio (idempotencia/estado) vive en el Aggregate, no aquí.
        receipt.Cancel();

        await _receiptRepository.UpdateAsync(receipt, cancellationToken);
    }
}
