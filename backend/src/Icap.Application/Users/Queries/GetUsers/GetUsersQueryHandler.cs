using Icap.Application.Users.DTOs;
using Icap.Application.Users.Mappers;
using Icap.Domain.Repositories;
using MediatR;

namespace Icap.Application.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users
            .OrderBy(u => u.FullName)
            .Select(u => u.ToDto())
            .ToList();
    }
}
