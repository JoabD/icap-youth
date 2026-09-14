using Icap.Domain.Exceptions;
using Icap.Domain.ValueObjects;
using Xunit;

namespace Icap.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Of_WithValidAmount_CreatesMoneyRoundedToTwoDecimals()
    {
        var money = Money.Of(150.456m, "MXN");

        Assert.Equal(150.46m, money.Amount);
        Assert.Equal("MXN", money.Currency);
    }

    [Fact]
    public void Of_NormalizesCurrencyToUpperInvariant()
    {
        var money = Money.Of(100m, "mxn");

        Assert.Equal("MXN", money.Currency);
    }

    [Fact]
    public void Of_DefaultsCurrencyToMXN()
    {
        var money = Money.Of(50m);

        Assert.Equal("MXN", money.Currency);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Of_WithNegativeAmount_ThrowsDomainException(decimal amount)
    {
        Assert.Throws<DomainException>(() => Money.Of(amount));
    }

    [Fact]
    public void Of_WithEmptyCurrency_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() => Money.Of(10m, ""));
    }

    [Fact]
    public void Zero_ReturnsMoneyWithAmountZero()
    {
        var money = Money.Zero();

        Assert.Equal(0m, money.Amount);
    }

    [Fact]
    public void Multiply_ByPositiveFactor_ReturnsScaledAmount()
    {
        var unitPrice = Money.Of(50m);

        var total = unitPrice.Multiply(3);

        Assert.Equal(150m, total.Amount);
    }

    [Fact]
    public void Multiply_ByNegativeFactor_ThrowsDomainException()
    {
        var unitPrice = Money.Of(50m);

        Assert.Throws<DomainException>(() => unitPrice.Multiply(-1));
    }

    [Fact]
    public void Add_WithSameCurrency_ReturnsSum()
    {
        var a = Money.Of(50m);
        var b = Money.Of(25m);

        var result = a.Add(b);

        Assert.Equal(75m, result.Amount);
    }

    [Fact]
    public void Add_WithDifferentCurrency_ThrowsDomainException()
    {
        var mxn = Money.Of(50m, "MXN");
        var usd = Money.Of(10m, "USD");

        Assert.Throws<DomainException>(() => mxn.Add(usd));
    }

    [Fact]
    public void Equals_WithSameAmountAndCurrency_ReturnsTrue()
    {
        var a = Money.Of(100m, "MXN");
        var b = Money.Of(100m, "MXN");

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Equals_WithDifferentAmount_ReturnsFalse()
    {
        var a = Money.Of(100m);
        var b = Money.Of(200m);

        Assert.NotEqual(a, b);
    }
}
