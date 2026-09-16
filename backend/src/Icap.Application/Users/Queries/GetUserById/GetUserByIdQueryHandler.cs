using Icap.Application.Common.Exceptions;
using Icap.Application.Users.DTOs;
using Icap.Application.Users.Mappers;
using Icap.Domain.Aggregates;
using Icap.Domain.Repositories;
using MediatR;

namespace Icap.Application.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetUserByIdQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        return user.ToDto();
    }
}
