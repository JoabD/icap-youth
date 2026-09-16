using Icap.Application.Receipts.Queries.GetReceipts;
using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using Icap.Domain.ValueObjects;
using Moq;
using Xunit;

namespace Icap.Application.Tests.Receipts;

public class GetReceiptsQueryHandlerTests
{
    private readonly Mock<IReceiptRepository> _receiptRepository = new();

    private GetReceiptsQueryHandler CreateHandler() => new(_receiptRepository.Object);

    private static Receipt IssueReceipt(string id, string createdBy) => Receipt.Issue(
        id, $"ICAP-{id}", "Juan Pérez", "Zona Norte", "juan@example.com", 2, Money.Of(50m), "qr-hash", createdBy);

    [Fact]
    public async Task Handle_WithoutCreatedByFilter_ReturnsAllReceipts()
    {
        var receipts = new[] { IssueReceipt("1", "user-1"), IssueReceipt("2", "user-2") };
        _receiptRepository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(receipts);

        var result = await CreateHandler().Handle(new GetReceiptsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        _receiptRepository.Verify(r => r.GetByCreatedByAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithCreatedByFilter_ReturnsOnlyThatDelegatesReceipts()
    {
        var receipts = new[] { IssueReceipt("1", "user-1") };
        _receiptRepository
            .Setup(r => r.GetByCreatedByAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(receipts);

        var result = await CreateHandler().Handle(new GetReceiptsQuery("user-1"), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("user-1", result[0].CreatedBy);
        _receiptRepository.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
