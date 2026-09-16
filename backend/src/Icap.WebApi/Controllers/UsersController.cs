using Icap.Application.Common.Interfaces;
using Icap.Application.Users.Commands.ActivateUser;
using Icap.Application.Users.Commands.ChangeUserPassword;
using Icap.Application.Users.Commands.CreateUser;
using Icap.Application.Users.Commands.DeactivateUser;
using Icap.Application.Users.Commands.UpdateUser;
using Icap.Application.Users.DTOs;
using Icap.Application.Users.Queries.GetUserById;
using Icap.Application.Users.Queries.GetUsers;
using Icap.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Icap.WebApi.Controllers;

/// <summary>
/// Administración de usuarios (altas/bajas/edición de Admins y Delegados).
/// Todo el controller es exclusivo de Admin: un Delegate no gestiona otras
/// cuentas, solo genera/consulta sus propios recibos (ver ReceiptsController).
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize(Roles = nameof(UserRole.Admin))]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUser;

    public UsersController(ISender mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUsersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetUserByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.FullName, request.Email, request.Password, request.Role);
        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> Update(string id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand(id, request.FullName, request.Email, request.Role);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>Restablece la contraseña de un usuario (uso administrativo: p. ej. un delegado olvidó su contraseña).</summary>
    [HttpPost("{id}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangeUserPasswordRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ChangeUserPasswordCommand(id, request.NewPassword), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(string id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new ActivateUserCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// "Elimina" un usuario. Es un soft-delete (desactivación) por diseño, no
    /// un borrado físico — ver el comentario en DeactivateUserCommand.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(string id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeactivateUserCommand(id, _currentUser.UserId!), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateUserRequest(string FullName, string Email, string Password, UserRole Role);

public sealed record UpdateUserRequest(string FullName, string Email, UserRole Role);

public sealed record ChangeUserPasswordRequest(string NewPassword);
