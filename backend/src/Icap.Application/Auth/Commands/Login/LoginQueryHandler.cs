using Icap.Application.Auth.DTOs;
using Icap.Application.Common.Exceptions;
using Icap.Domain.Repositories;
using Icap.Domain.Services;
using MediatR;

namespace Icap.Application.Auth.Commands.Login;

/// <summary>
/// Orquesta el caso de uso de autenticación:
/// 1) busca al usuario por email, 2) valida contraseña e IsActive,
/// 3) delega en Infrastructure la generación del JWT.
/// No conoce MongoDB ni el algoritmo de hashing/JWT: solo las interfaces del dominio.
/// </summary>
public sealed class LoginQueryHandler : IRequestHandler<LoginQuery, LoginResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginQueryHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResultDto> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessAppException("Correo o contraseña incorrectos.");
        }

        // Regla de negocio del Aggregate: usuarios inactivos no pueden autenticarse.
        user.EnsureCanAuthenticate();

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new LoginResultDto(
            AccessToken: token,
            UserId: user.Id,
            FullName: user.FullName,
            Email: user.Email.Value,
            Role: user.Role.ToString());
    }
}
