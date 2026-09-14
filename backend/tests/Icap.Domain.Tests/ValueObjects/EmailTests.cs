using Icap.Domain.Exceptions;
using Icap.Domain.ValueObjects;
using Xunit;

namespace Icap.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("delegado@icapjuvenil.org")]
    [InlineData("Admin@ICAP.ORG")]
    [InlineData("nombre.apellido+tag@dominio.com.mx")]
    public void Of_WithValidFormat_CreatesEmail(string rawEmail)
    {
        var email = Email.Of(rawEmail);

        Assert.Equal(rawEmail.Trim().ToLowerInvariant(), email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("no-es-un-correo")]
    [InlineData("falta-dominio@")]
    [InlineData("@falta-usuario.com")]
    public void Of_WithInvalidFormat_ThrowsDomainException(string rawEmail)
    {
        Assert.Throws<DomainException>(() => Email.Of(rawEmail));
    }

    [Fact]
    public void Of_IsCaseInsensitiveForEquality()
    {
        var a = Email.Of("Delegado@ICAPJuvenil.org");
        var b = Email.Of("delegado@icapjuvenil.org");

        Assert.Equal(a, b);
    }

    [Fact]
    public void ToString_ReturnsNormalizedValue()
    {
        var email = Email.Of("Test@Example.COM");

        Assert.Equal("test@example.com", email.ToString());
    }
}
