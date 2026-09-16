using Icap.Application.Common.Exceptions;
using Icap.Application.Receipts.Mappers;
using Icap.Domain.Aggregates;
using Icap.Domain.Exceptions;
using Icap.Domain.Repositories;
using Icap.Domain.Services;
using MediatR;

namespace Icap.Application.Receipts.Commands.SendReceiptEmail;

public sealed class SendReceiptEmailCommandHandler : IRequestHandler<SendReceiptEmailCommand>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IReceiptPdfGenerator _pdfGenerator;
    private readonly IEmailSender _emailSender;

    public SendReceiptEmailCommandHandler(
        IReceiptRepository receiptRepository,
        IReceiptPdfGenerator pdfGenerator,
        IEmailSender emailSender)
    {
        _receiptRepository = receiptRepository;
        _pdfGenerator = pdfGenerator;
        _emailSender = emailSender;
    }

    public async Task Handle(SendReceiptEmailCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _receiptRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Receipt), request.Id);

        if (receipt.DelegateInfo.Email is null)
        {
            throw new DomainException(
                "Este recibo no tiene un correo de delegado registrado (se emitió antes de que este campo existiera), así que no se puede enviar por email.");
        }

        var pdfBytes = _pdfGenerator.Generate(receipt);
        var message = ReceiptEmailContentFactory.BuildReceiptEmail(receipt, pdfBytes);

        await _emailSender.SendAsync(message, cancellationToken);
    }
}
