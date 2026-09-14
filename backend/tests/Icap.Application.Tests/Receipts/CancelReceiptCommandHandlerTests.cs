using Icap.Application.Common.Exceptions;
using Icap.Application.Receipts.Commands.CancelReceipt;
using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using Icap.Domain.ValueObjects;
using Moq;
using Xunit;

namespace Icap.Application.Tests.Receipts;

public class CancelReceiptCommandHandlerTests
{
    private readonly Mock<IReceiptRepository> _receiptRepository = new();

    private CancelReceiptCommandHandler CreateHandler() => new(_receiptRepository.Object);

    private static Receipt IssueValidReceipt() => Receipt.Issue(
        "receipt-1", "ICAP-000001", "Juan Pérez", "Zona Norte", 5, Money.Of(50m), "qr-hash", "user-1");

    [Fact]
    public async Task Handle_WithExistingIssuedReceipt_CancelsAndPersistsIt()
    {
        var receipt = IssueValidReceipt();
        _receiptRepository.Setup(r => r.GetByIdAsync(receipt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);

        await CreateHandler().Handle(new CancelReceiptCommand(receipt.Id), CancellationToken.None);

        Assert.False(receipt.IsValidForRedemption());
        _receiptRepository.Verify(r => r.UpdateAsync(receipt, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnknownReceiptId_ThrowsNotFoundException()
    {
        _receiptRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Receipt?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new CancelReceiptCommand("no-existe"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithAlreadyCancelledReceipt_ThrowsDomainExceptionAndDoesNotUpdate()
    {
        var receipt = IssueValidReceipt();
        receipt.Cancel();
        _receiptRepository.Setup(r => r.GetByIdAsync(receipt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);

        await Assert.ThrowsAsync<Icap.Domain.Exceptions.DomainException>(() =>
            CreateHandler().Handle(new CancelReceiptCommand(receipt.Id), CancellationToken.None));

        _receiptRepository.Verify(r => r.UpdateAsync(It.IsAny<Receipt>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
