using Icap.Application.Auth.DTOs;
using MediatR;

namespace Icap.Application.Auth.Commands.Login;

/// <summary>
/// Petición de autenticación. Se nombra "LoginQuery" siguiendo el Documento
/// de Diseño del Sistema; conceptualmente actúa como un Command dentro del
/// pipeline de MediatR (produce un efecto observable: emite un JWT), pero
/// no muta el estado persistido del dominio, de ahí la convención de nombre.
/// </summary>
public sealed record LoginQuery(string Email, string Password) : IRequest<LoginResultDto>;
