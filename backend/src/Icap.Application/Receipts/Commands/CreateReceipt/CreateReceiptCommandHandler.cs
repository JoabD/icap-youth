using Icap.Application.Common.Exceptions;
using Icap.Application.Receipts.DTOs;
using Icap.Application.Receipts.Mappers;
using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using Icap.Domain.Services;
using Icap.Domain.ValueObjects;
using MediatR;

namespace Icap.Application.Receipts.Commands.CreateReceipt;

/// <summary>
/// Orquesta la emisión de un recibo:
/// 1) resuelve el folio (IFolioNumberGenerator),
/// 2) resuelve el Id y el QRHash (IQrHashGenerator),
/// 3) construye el Aggregate Receipt (reglas de negocio en el dominio),
/// 4) persiste vía IReceiptRepository.
/// </summary>
public sealed class CreateReceiptCommandHandler : IRequestHandler<CreateReceiptCommand, ReceiptDto>
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly IUserRepository _userRepository;
    private readonly IFolioNumberGenerator _folioNumberGenerator;
    private readonly IQrHashGenerator _qrHashGenerator;

    public CreateReceiptCommandHandler(
        IReceiptRepository receiptRepository,
        IUserRepository userRepository,
        IFolioNumberGenerator folioNumberGenerator,
        IQrHashGenerator qrHashGenerator)
    {
        _receiptRepository = receiptRepository;
        _userRepository = userRepository;
        _folioNumberGenerator = folioNumberGenerator;
        _qrHashGenerator = qrHashGenerator;
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
            wristbandsQuantity: request.WristbandsQuantity,
            unitPrice: unitPrice,
            qrHash: qrHash,
            createdBy: createdBy.Id);

        await _receiptRepository.AddAsync(receipt, cancellationToken);

        return receipt.ToDto();
    }
}
