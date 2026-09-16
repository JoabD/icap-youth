using Icap.Application.Users.DTOs;
using Icap.Application.Users.Mappers;
using Icap.Domain.Aggregates;
using Icap.Domain.Exceptions;
using Icap.Domain.Repositories;
using Icap.Domain.Services;
using MediatR;

namespace Icap.Application.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
        {
            throw new DomainException($"Ya existe un usuario registrado con el correo '{request.Email}'.");
        }

        var user = User.Create(
            id: Guid.NewGuid().ToString("N"),
            fullName: request.FullName,
            email: request.Email,
            passwordHash: _passwordHasher.Hash(request.Password),
            role: request.Role);

        await _userRepository.AddAsync(user, cancellationToken);

        return user.ToDto();
    }
}
