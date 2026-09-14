using FluentValidation;

namespace Icap.Application.Receipts.Commands.CancelReceipt;

public sealed class CancelReceiptCommandValidator : AbstractValidator<CancelReceiptCommand>
{
    public CancelReceiptCommandValidator()
    {
        RuleFor(x => x.ReceiptId)
            .NotEmpty().WithMessage("El Id del recibo es requerido.");
    }
}
