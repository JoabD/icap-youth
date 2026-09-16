using Icap.Application.Common.Exceptions;
using Icap.Application.Receipts.Commands.CreateReceipt;
using Icap.Domain.Aggregates;
using Icap.Domain.Enums;
using Icap.Domain.Repositories;
using Icap.Domain.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Icap.Application.Tests.Receipts;

public class CreateReceiptCommandHandlerTests
{
    private readonly Mock<IReceiptRepository> _receiptRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IFolioNumberGenerator> _folioNumberGenerator = new();
    private readonly Mock<IQrHashGenerator> _qrHashGenerator = new();
    private readonly Mock<IReceiptPdfGenerator> _pdfGenerator = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<ILogger<CreateReceiptCommandHandler>> _logger = new();

    private CreateReceiptCommandHandler CreateHandler() => new(
        _receiptRepository.Object,
        _userRepository.Object,
        _folioNumberGenerator.Object,
        _qrHashGenerator.Object,
        _pdfGenerator.Object,
        _emailSender.Object,
        _logger.Object);

    private static User CreateActiveDelegate() =>
        User.Create("user-1", "Juan Pérez", "juan@icapjuvenil.org", "hash", UserRole.Delegate);

    [Fact]
    public async Task Handle_WithValidCommand_IssuesReceiptAndPersistsIt()
    {
        var user = CreateActiveDelegate();
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _folioNumberGenerator.Setup(f => f.NextAsync(It.IsAny<CancellationToken>())).ReturnsAsync("ICAP-000001");
        _qrHashGenerator.Setup(q => q.Generate("ICAP-000001", It.IsAny<string>())).Returns("qr-hash-abc");

        var command = new CreateReceiptCommand(
            DelegateName: "Juan Pérez",
            AreaOrRegion: "Zona Norte",
            DelegateEmail: "juan.delegado@example.com",
            WristbandsQuantity: 4,
            UnitPrice: 75m,
            CreatedByUserId: user.Id);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.Equal("ICAP-000001", result.FolioNumber);
        Assert.Equal(4, result.WristbandsQuantity);
        Assert.Equal(300m, result.TotalCost);
        Assert.Equal("qr-hash-abc", result.QRHash);
        Assert.Equal("Issued", result.Status);

        _receiptRepository.Verify(r => r.AddAsync(
            It.Is<Receipt>(receipt => receipt.FolioNumber == "ICAP-000001"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnknownCreatedByUserId_ThrowsNotFoundException()
    {
        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new CreateReceiptCommand("Juan Pérez", "Zona Norte", "juan.delegado@example.com", 4, 75m, "usuario-inexistente");

        await Assert.ThrowsAsync<NotFoundException>(() => CreateHandler().Handle(command, CancellationToken.None));

        _receiptRepository.Verify(r => r.AddAsync(It.IsAny<Receipt>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveCreator_ThrowsDomainException()
    {
        var user = CreateActiveDelegate();
        user.Deactivate();
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var command = new CreateReceiptCommand("Juan Pérez", "Zona Norte", "juan.delegado@example.com", 4, 75m, user.Id);

        await Assert.ThrowsAsync<Icap.Domain.Exceptions.DomainException>(() =>
            CreateHandler().Handle(command, CancellationToken.None));
    }
}
