using Icap.Application.Common.Exceptions;
using Icap.Application.Receipts.DTOs;
using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using Icap.Domain.Services;
using MediatR;

namespace Icap.Application.Receipts.Queries.GetReceiptPdf;

public sealed class GetReceiptPdfQueryHandler : IRequestHandler<GetReceiptPdfQuery, ReceiptPdfDto>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IReceiptPdfGenerator _pdfGenerator;

    public GetReceiptPdfQueryHandler(IReceiptRepository receiptRepository, IReceiptPdfGenerator pdfGenerator)
    {
        _receiptRepository = receiptRepository;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<ReceiptPdfDto> Handle(GetReceiptPdfQuery request, CancellationToken cancellationToken)
    {
        var receipt = await _receiptRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Receipt), request.Id);

        var pdfBytes = _pdfGenerator.Generate(receipt);
        var fileName = $"Recibo-{receipt.FolioNumber}.pdf";

        return new ReceiptPdfDto(pdfBytes, fileName);
    }
}
