using Icap.Application.Common.Exceptions;
using Icap.Application.Users.DTOs;
using Icap.Application.Users.Mappers;
using Icap.Domain.Aggregates;
using Icap.Domain.Exceptions;
using Icap.Domain.Repositories;
using MediatR;

namespace Icap.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        var emailChanged = !string.Equals(user.Email.Value, request.Email.Trim(), StringComparison.OrdinalIgnoreCase);

        if (emailChanged && await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new DomainException($"Ya existe un usuario registrado con el correo '{request.Email}'.");
        }

        user.UpdateProfile(request.FullName, request.Email);
        user.ChangeRole(request.Role);

        await _userRepository.UpdateAsync(user, cancellationToken);

        return user.ToDto();
    }
}
