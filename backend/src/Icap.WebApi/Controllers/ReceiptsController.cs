using Icap.Application.Common.Interfaces;
using Icap.Application.Receipts.Commands.CancelReceipt;
using Icap.Application.Receipts.Commands.CreateReceipt;
using Icap.Application.Receipts.DTOs;
using Icap.Application.Receipts.Queries.GetReceiptById;
using Icap.Application.Receipts.Queries.GetReceipts;
using Icap.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Icap.WebApi.Controllers;

/// <summary>
/// Endpoints de recibos. Todo el controller requiere autenticación
/// ([Authorize] a nivel de clase); las reglas de "quién ve qué" (Admin ve
/// todos, Delegate solo los suyos) se resuelven aquí usando
/// ICurrentUserService, sin duplicar esa lógica en el frontend.
/// </summary>
[ApiController]
[Route("api/v1/receipts")]
[Authorize]
public sealed class ReceiptsController : ControllerBase
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUser;

    public ReceiptsController(ISender mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>Lista los recibos: un Admin ve todos, un Delegate solo los que él generó.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReceiptDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReceiptDto>>> GetAll(CancellationToken cancellationToken)
    {
        var createdByFilter = _currentUser.Role == UserRole.Admin ? null : _currentUser.UserId;

        var result = await _mediator.Send(new GetReceiptsQuery(createdByFilter), cancellationToken);
        return Ok(result);
    }

    /// <summary>Obtiene un recibo por Id (usado por la vista de impresión en Angular).</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ReceiptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReceiptDto>> GetById(string id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetReceiptByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Emite un nuevo recibo. CreatedByUserId siempre se toma del JWT, nunca del body.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReceiptDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReceiptDto>> Create([FromBody] CreateReceiptRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateReceiptCommand(
            request.DelegateName,
            request.AreaOrRegion,
            request.WristbandsQuantity,
            request.UnitPrice,
            _currentUser.UserId!);

        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Cancela (anula) un recibo emitido. Solo Admin puede cancelar.</summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(string id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelReceiptCommand(id), cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// Request de creación expuesto por la API (sin CreatedByUserId: ese dato
/// lo agrega el controller desde el JWT, no el cliente).
/// </summary>
public sealed record CreateReceiptRequest(
    string DelegateName,
    string AreaOrRegion,
    int WristbandsQuantity,
    decimal UnitPrice);
