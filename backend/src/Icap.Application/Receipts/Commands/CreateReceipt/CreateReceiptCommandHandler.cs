using Icap.Application.Common.Exceptions;
using Icap.Application.Receipts.DTOs;
using Icap.Application.Receipts.Mappers;
using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using Icap.Domain.Services;
using Icap.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Icap.Application.Receipts.Commands.CreateReceipt;

/// <summary>
/// Orquesta la emisión de un recibo:
/// 1) resuelve el folio (IFolioNumberGenerator),
/// 2) resuelve el Id y el QRHash (IQrHashGenerator),
/// 3) construye el Aggregate Receipt (reglas de negocio en el dominio),
/// 4) persiste vía IReceiptRepository,
/// 5) intenta enviar el PDF por correo al delegado (best-effort: si el SMTP
///    falla o está mal configurado, el recibo YA quedó creado y válido —
///    el error de envío no debe convertirse en un 500 para el usuario que
///    solo quería generar su recibo; puede reenviarlo después con
///    SendReceiptEmailCommand).
/// </summary>
public sealed class CreateReceiptCommandHandler : IRequestHandler<CreateReceiptCommand, ReceiptDto>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFolioNumberGenerator _folioNumberGenerator;
    private readonly IQrHashGenerator _qrHashGenerator;
    private readonly IReceiptPdfGenerator _pdfGenerator;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<CreateReceiptCommandHandler> _logger;

    public CreateReceiptCommandHandler(
        IReceiptRepository receiptRepository,
        IUserRepository userRepository,
        IFolioNumberGenerator folioNumberGenerator,
        IQrHashGenerator qrHashGenerator,
        IReceiptPdfGenerator pdfGenerator,
        IEmailSender emailSender,
        ILogger<CreateReceiptCommandHandler> logger)
    {
        _receiptRepository = receiptRepository;
        _userRepository = userRepository;
        _folioNumberGenerator = folioNumberGenerator;
        _qrHashGenerator = qrHashGenerator;
        _pdfGenerator = pdfGenerator;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<ReceiptDto> Handle(CreateReceiptCommand request, CancellationToken cancellationToken)
    {
        var createdBy = await _userRepository.GetByIdAsync(request.CreatedByUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.CreatedByUserId);

        createdBy.EnsureCanAuthenticate();

        var receiptId = Guid.NewGuid().ToString("N");
        var folioNumber = await _folioNumberGenerator.NextAsync(cancellationToken);
        var qrHash = _qrHashGenerator.Generate(folioNumber, receiptId);
        var unitPrice = Money.Of(request.UnitPrice);

        var receipt = Receipt.Issue(
            id: receiptId,
            folioNumber: folioNumber,
            delegateName: request.DelegateName,
            areaOrRegion: request.AreaOrRegion,
            delegateEmail: request.DelegateEmail,
            wristbandsQuantity: request.WristbandsQuantity,
            unitPrice: unitPrice,
            qrHash: qrHash,
            createdBy: createdBy.Id);

        await _receiptRepository.AddAsync(receipt, cancellationToken);

        try
        {
            var pdfBytes = _pdfGenerator.Generate(receipt);
            var message = ReceiptEmailContentFactory.BuildReceiptEmail(receipt, pdfBytes);
            await _emailSender.SendAsync(message, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "No se pudo enviar por correo el recibo {FolioNumber} recién creado. El recibo sí quedó guardado; se puede reenviar manualmente.",
                receipt.FolioNumber);
        }

        return receipt.ToDto();
    }
}
