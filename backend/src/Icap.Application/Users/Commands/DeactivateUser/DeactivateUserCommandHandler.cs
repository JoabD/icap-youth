using Icap.Application.Common.Exceptions;
using Icap.Domain.Aggregates;
using Icap.Domain.Enums;
using Icap.Domain.Exceptions;
using Icap.Domain.Repositories;
using MediatR;

namespace Icap.Application.Users.Commands.DeactivateUser;

public sealed class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeactivateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == request.RequestedByUserId)
        {
            throw new DomainException("No puedes desactivar tu propia cuenta.");
        }

        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        if (user.Role == UserRole.Admin)
        {
            var allUsers = await _userRepository.GetAllAsync(cancellationToken);
            var otherActiveAdmins = allUsers.Any(u => u.Id != user.Id && u.Role == UserRole.Admin && u.IsActive);

            if (!otherActiveAdmins)
            {
                throw new DomainException("No puedes desactivar al único administrador activo de la plataforma.");
            }
        }

        user.Deactivate();

        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}
