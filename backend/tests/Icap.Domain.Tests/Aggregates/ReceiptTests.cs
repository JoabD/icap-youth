using Icap.Domain.Aggregates;
using Icap.Domain.Enums;
using Icap.Domain.Exceptions;
using Icap.Domain.ValueObjects;
using Xunit;

namespace Icap.Domain.Tests.Aggregates;

public class ReceiptTests
{
    private static Receipt IssueValidReceipt(int quantity = 10, decimal unitPrice = 50m) =>
        Receipt.Issue(
            id: "receipt-1",
            folioNumber: "ICAP-000001",
            delegateName: "María García",
            areaOrRegion: "Zona Norte",
            delegateEmail: "maria@example.com",
            wristbandsQuantity: quantity,
            unitPrice: Money.Of(unitPrice),
            qrHash: "qr-hash-123",
            createdBy: "user-1");

    [Fact]
    public void Issue_WithValidData_ComputesTotalCostFromUnitPriceAndQuantity()
    {
        var receipt = IssueValidReceipt(quantity: 10, unitPrice: 50m);

        Assert.Equal(10, receipt.PurchaseDetails.WristbandsQuantity);
        Assert.Equal(50m, receipt.PurchaseDetails.UnitPrice.Amount);
        Assert.Equal(500m, receipt.PurchaseDetails.TotalCost.Amount);
    }

    [Fact]
    public void Issue_SetsStatusIssuedAndCreatedAtInUtc()
    {
        var before = DateTime.UtcNow;

        var receipt = IssueValidReceipt();

        var after = DateTime.UtcNow;

        Assert.Equal(ReceiptStatus.Issued, receipt.Validation.Status);
        Assert.True(receipt.IsValidForRedemption());
        Assert.InRange(receipt.CreatedAt, before.AddSeconds(-1), after.AddSeconds(1));
    }

    [Fact]
    public void Issue_PreservesDelegateSnapshotIndependentlyOfUserAggregate()
    {
        var receipt = IssueValidReceipt();

        Assert.Equal("María García", receipt.DelegateInfo.DelegateName);
        Assert.Equal("Zona Norte", receipt.DelegateInfo.AreaOrRegion);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Issue_WithNonPositiveQuantity_ThrowsDomainException(int invalidQuantity)
    {
        Assert.Throws<DomainException>(() => IssueValidReceipt(quantity: invalidQuantity));
    }

    [Fact]
    public void Issue_WithoutCreatedBy_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Receipt.Issue(
            "receipt-1", "ICAP-000001", "María García", "Zona Norte", "maria@example.com", 10, Money.Of(50m), "qr-hash", createdBy: " "));
    }

    [Fact]
    public void Cancel_WhenIssued_SetsStatusCancelled()
    {
        var receipt = IssueValidReceipt();

        receipt.Cancel();

        Assert.Equal(ReceiptStatus.Cancelled, receipt.Validation.Status);
        Assert.False(receipt.IsValidForRedemption());
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsDomainException()
    {
        var receipt = IssueValidReceipt();
        receipt.Cancel();

        Assert.Throws<DomainException>(() => receipt.Cancel());
    }

    [Fact]
    public void Rehydrate_WithCancelledStatus_ReconstructsCancelledReceipt()
    {
        var receipt = Receipt.Rehydrate(
            id: "receipt-2",
            folioNumber: "ICAP-000002",
            delegateName: "Luis Torres",
            areaOrRegion: "Zona Sur",
            delegateEmail: "luis@example.com",
            wristbandsQuantity: 5,
            unitPriceAmount: 60m,
            currency: "MXN",
            qrHash: "qr-hash-456",
            status: ReceiptStatus.Cancelled,
            createdAt: DateTime.UtcNow.AddDays(-1),
            createdBy: "user-2");

        Assert.Equal(ReceiptStatus.Cancelled, receipt.Validation.Status);
        Assert.False(receipt.IsValidForRedemption());
        Assert.Equal(300m, receipt.PurchaseDetails.TotalCost.Amount);
    }
}
