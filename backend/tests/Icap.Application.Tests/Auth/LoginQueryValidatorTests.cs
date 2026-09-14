using Icap.Application.Auth.Commands.Login;
using Xunit;

namespace Icap.Application.Tests.Auth;

public class LoginQueryValidatorTests
{
    private readonly LoginQueryValidator _validator = new();

    [Fact]
    public void Validate_WithValidQuery_HasNoErrors()
    {
        var result = _validator.Validate(new LoginQuery("juan@icapjuvenil.org", "P@ssw0rd"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "P@ssw0rd")]
    [InlineData("correo-invalido", "P@ssw0rd")]
    [InlineData("juan@icapjuvenil.org", "")]
    public void Validate_WithInvalidData_HasErrors(string email, string password)
    {
        var result = _validator.Validate(new LoginQuery(email, password));

        Assert.False(result.IsValid);
    }
}
