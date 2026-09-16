using Icap.Application.Common.Exceptions;
using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using Icap.Domain.Services;
using MediatR;

namespace Icap.Application.Users.Commands.ChangeUserPassword;

public sealed class ChangeUserPasswordCommandHandler : IRequestHandler<ChangeUserPasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangeUserPasswordCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(ChangeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        user.ChangePasswordHash(_passwordHasher.Hash(request.NewPassword));

        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}
