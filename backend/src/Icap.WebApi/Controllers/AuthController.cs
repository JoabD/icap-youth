using Icap.Application.Auth.Commands.Login;
using Icap.Application.Auth.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Icap.WebApi.Controllers;

/// <summary>Endpoints de autenticación. Sin [Authorize]: es el punto de entrada público.</summary>
[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Autentica a un Admin o Delegate y devuelve un JWT.</summary>
    /// <response code="200">Login exitoso.</response>
    /// <response code="401">Credenciales inválidas.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResultDto>> Login([FromBody] LoginQuery query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
