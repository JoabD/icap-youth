using Icap.Domain.Aggregates;
using Icap.Domain.Enums;
using Icap.Domain.Exceptions;
using Xunit;

namespace Icap.Domain.Tests.Aggregates;

public class UserTests
{
    private static User CreateValidUser(UserRole role = UserRole.Delegate) =>
        User.Create(
            id: "user-1",
            fullName: "Juan Pérez",
            email: "juan.perez@icapjuvenil.org",
            passwordHash: "hashed-password",
            role: role);

    [Fact]
    public void Create_WithValidData_ReturnsActiveUser()
    {
        var user = CreateValidUser();

        Assert.Equal("user-1", user.Id);
        Assert.Equal("Juan Pérez", user.FullName);
        Assert.Equal("juan.perez@icapjuvenil.org", user.Email.Value);
        Assert.True(user.IsActive);
        Assert.Equal(UserRole.Delegate, user.Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithoutId_ThrowsDomainException(string invalidId)
    {
        Assert.Throws<DomainException>(() =>
            User.Create(invalidId, "Juan Pérez", "juan@icap.org", "hash", UserRole.Delegate));
    }

    [Fact]
    public void Create_WithoutFullName_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            User.Create("user-1", "  ", "juan@icap.org", "hash", UserRole.Delegate));
    }

    [Fact]
    public void Create_WithInvalidEmail_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            User.Create("user-1", "Juan Pérez", "correo-invalido", "hash", UserRole.Delegate));
    }

    [Fact]
    public void Deactivate_WhenActive_SetsIsActiveFalse()
    {
        var user = CreateValidUser();

        user.Deactivate();

        Assert.False(user.IsActive);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ThrowsDomainException()
    {
        var user = CreateValidUser();
        user.Deactivate();

        Assert.Throws<DomainException>(() => user.Deactivate());
    }

    [Fact]
    public void EnsureCanAuthenticate_WhenActive_DoesNotThrow()
    {
        var user = CreateValidUser();

        var exception = Record.Exception(() => user.EnsureCanAuthenticate());

        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanAuthenticate_WhenInactive_ThrowsDomainException()
    {
        var user = CreateValidUser();
        user.Deactivate();

        Assert.Throws<DomainException>(() => user.EnsureCanAuthenticate());
    }

    [Fact]
    public void SetExtraAttribute_AddsKeyValuePair()
    {
        var user = CreateValidUser();

        user.SetExtraAttribute("talla_playera", "M");

        Assert.Equal("M", user.ExtraAttributes["talla_playera"]);
    }

    [Fact]
    public void ChangePasswordHash_WithEmptyValue_ThrowsDomainException()
    {
        var user = CreateValidUser();

        Assert.Throws<DomainException>(() => user.ChangePasswordHash(""));
    }

    [Fact]
    public void Rehydrate_PreservesInactiveStateAndExtraAttributes()
    {
        var extraAttributes = new Dictionary<string, object> { ["talla_playera"] = "L" };

        var user = User.Rehydrate(
            id: "user-2",
            fullName: "Ana López",
            email: "ana.lopez@icapjuvenil.org",
            passwordHash: "hash",
            role: UserRole.Admin,
            isActive: false,
            extraAttributes: extraAttributes);

        Assert.False(user.IsActive);
        Assert.Equal(UserRole.Admin, user.Role);
        Assert.Equal("L", user.ExtraAttributes["talla_playera"]);
    }
}
