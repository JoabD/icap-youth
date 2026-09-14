using Icap.Application.Auth.Commands.Login;
using Icap.Application.Common.Exceptions;
using Icap.Domain.Aggregates;
using Icap.Domain.Enums;
using Icap.Domain.Repositories;
using Icap.Domain.Services;
using Moq;
using Xunit;

namespace Icap.Application.Tests.Auth;

public class LoginQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();

    private LoginQueryHandler CreateHandler() =>
        new(_userRepository.Object, _passwordHasher.Object, _jwtTokenGenerator.Object);

    private static User CreateActiveUser(UserRole role = UserRole.Delegate) =>
        User.Create("user-1", "Juan Pérez", "juan.perez@icapjuvenil.org", "hashed-password", role);

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsLoginResultWithToken()
    {
        var user = CreateActiveUser();
        _userRepository.Setup(r => r.GetByEmailAsync(user.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("correct-password", user.PasswordHash)).Returns(true);
        _jwtTokenGenerator.Setup(j => j.GenerateToken(user)).Returns("fake-jwt-token");

        var result = await CreateHandler().Handle(
            new LoginQuery(user.Email.Value, "correct-password"), CancellationToken.None);

        Assert.Equal("fake-jwt-token", result.AccessToken);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.FullName, result.FullName);
        Assert.Equal("Delegate", result.Role);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ThrowsUnauthorizedAccessAppException()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessAppException>(() =>
            CreateHandler().Handle(new LoginQuery("no-existe@icapjuvenil.org", "cualquiera"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsUnauthorizedAccessAppException()
    {
        var user = CreateActiveUser();
        _userRepository.Setup(r => r.GetByEmailAsync(user.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("wrong-password", user.PasswordHash)).Returns(false);

        await Assert.ThrowsAsync<UnauthorizedAccessAppException>(() =>
            CreateHandler().Handle(new LoginQuery(user.Email.Value, "wrong-password"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ThrowsDomainException()
    {
        var user = CreateActiveUser();
        user.Deactivate();

        _userRepository.Setup(r => r.GetByEmailAsync(user.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("correct-password", user.PasswordHash)).Returns(true);

        await Assert.ThrowsAsync<Icap.Domain.Exceptions.DomainException>(() =>
            CreateHandler().Handle(new LoginQuery(user.Email.Value, "correct-password"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NeverCallsTokenGenerator_WhenCredentialsAreInvalid()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessAppException>(() =>
            CreateHandler().Handle(new LoginQuery("no-existe@icapjuvenil.org", "x"), CancellationToken.None));

        _jwtTokenGenerator.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }
}
