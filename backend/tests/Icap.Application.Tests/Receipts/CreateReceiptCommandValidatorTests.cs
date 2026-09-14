using Icap.Application.Receipts.Commands.CreateReceipt;
using Xunit;

namespace Icap.Application.Tests.Receipts;

public class CreateReceiptCommandValidatorTests
{
    private readonly CreateReceiptCommandValidator _validator = new();

    private static CreateReceiptCommand ValidCommand() => new(
        DelegateName: "Juan Pérez",
        AreaOrRegion: "Zona Norte",
        WristbandsQuantity: 5,
        UnitPrice: 50m,
        CreatedByUserId: "user-1");

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        var result = _validator.Validate(ValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithEmptyDelegateName_HasError()
    {
        var command = ValidCommand() with { DelegateName = "" };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReceiptCommand.DelegateName));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Validate_WithInvalidWristbandsQuantity_HasError(int quantity)
    {
        var command = ValidCommand() with { WristbandsQuantity = quantity };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReceiptCommand.WristbandsQuantity));
    }

    [Fact]
    public void Validate_WithNonPositiveUnitPrice_HasError()
    {
        var command = ValidCommand() with { UnitPrice = 0m };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateReceiptCommand.UnitPrice));
    }

    [Fact]
    public void Validate_WithoutCreatedByUserId_HasError()
    {
        var command = ValidCommand() with { CreatedByUserId = "" };

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
