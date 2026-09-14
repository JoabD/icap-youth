using Icap.Application.Common.Exceptions;
using Icap.Application.Receipts.Queries.GetReceiptById;
using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using Icap.Domain.ValueObjects;
using Moq;
using Xunit;

namespace Icap.Application.Tests.Receipts;

public class GetReceiptByIdQueryHandlerTests
{
    private readonly Mock<IReceiptRepository> _receiptRepository = new();

    private GetReceiptByIdQueryHandler CreateHandler() => new(_receiptRepository.Object);

    [Fact]
    public async Task Handle_WithExistingReceipt_ReturnsMappedDto()
    {
        var receipt = Receipt.Issue(
            "receipt-1", "ICAP-000001", "Juan Pérez", "Zona Norte", 3, Money.Of(80m), "qr-hash", "user-1");

        _receiptRepository.Setup(r => r.GetByIdAsync(receipt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);

        var dto = await CreateHandler().Handle(new GetReceiptByIdQuery(receipt.Id), CancellationToken.None);

        Assert.Equal(receipt.Id, dto.Id);
        Assert.Equal(receipt.FolioNumber, dto.FolioNumber);
        Assert.Equal(240m, dto.TotalCost);
    }

    [Fact]
    public async Task Handle_WithUnknownReceipt_ThrowsNotFoundException()
    {
        _receiptRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Receipt?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            CreateHandler().Handle(new GetReceiptByIdQuery("no-existe"), CancellationToken.None));
    }
}
