using Icap.Application.Common.Exceptions;
using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using MediatR;

namespace Icap.Application.Users.Commands.ActivateUser;

public sealed class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand>
{
    private readonly IUserRepository _userRepository;

    public ActivateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        user.Activate();

        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}
