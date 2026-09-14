using FluentValidation;
using FluentValidation.Results;
using Icap.Application.Common.Behaviors;
using MediatR;
using Moq;
using Xunit;
using ValidationException = Icap.Application.Common.Exceptions.ValidationException;

namespace Icap.Application.Tests.Common;

public class ValidationBehaviorTests
{
    public sealed record SampleRequest(string Name) : IRequest<string>;

    [Fact]
    public async Task Handle_WithoutValidators_CallsNextDirectly()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>(Array.Empty<IValidator<SampleRequest>>());
        var nextCalled = false;

        var result = await behavior.Handle(new SampleRequest("ok"), () =>
        {
            nextCalled = true;
            return Task.FromResult("resultado");
        }, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal("resultado", result);
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ThrowsValidationExceptionAndNeverCallsNext()
    {
        var validator = new Mock<IValidator<SampleRequest>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "El nombre es requerido.") }));

        var behavior = new ValidationBehavior<SampleRequest, string>(new[] { validator.Object });
        var nextCalled = false;

        await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(new SampleRequest(""), () =>
        {
            nextCalled = true;
            return Task.FromResult("no debería llegar aquí");
        }, CancellationToken.None));

        Assert.False(nextCalled);
    }

    [Fact]
    public async Task Handle_WithPassingValidator_CallsNext()
    {
        var validator = new Mock<IValidator<SampleRequest>>();
        validator
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<SampleRequest, string>(new[] { validator.Object });

        var result = await behavior.Handle(new SampleRequest("ok"), () => Task.FromResult("resultado"), CancellationToken.None);

        Assert.Equal("resultado", result);
    }
}
