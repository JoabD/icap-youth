using FluentValidation;

namespace Icap.Application.Receipts.Commands.CreateReceipt;

public sealed class CreateReceiptCommandValidator : AbstractValidator<CreateReceiptCommand>
{
    public CreateReceiptCommandValidator()
    {
        RuleFor(x => x.DelegateName)
            .NotEmpty().WithMessage("El nombre del delegado es requerido.")
            .MaximumLength(150);

        RuleFor(x => x.AreaOrRegion)
            .NotEmpty().WithMessage("El área o región es requerida.")
            .MaximumLength(100);

        RuleFor(x => x.WristbandsQuantity)
            .GreaterThan(0).WithMessage("La cantidad de pulseras debe ser mayor a cero.")
            .LessThanOrEqualTo(1000).WithMessage("La cantidad de pulseras excede el máximo permitido por recibo.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a cero.");

        RuleFor(x => x.CreatedByUserId)
            .NotEmpty().WithMessage("No se pudo determinar el usuario que genera el recibo.");
    }
}
