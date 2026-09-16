using MediatR;

namespace Icap.Application.Users.Commands.DeactivateUser;

/// <summary>
/// "Elimina" un usuario en el sentido de negocio: lo desactiva (soft-delete)
/// en vez de borrarlo físicamente de Mongo. Un delete físico dejaría
/// huérfano el histórico de Receipts.CreatedBy (que referencia este Id) y
/// perdería el registro de quién generó qué recibo — el dominio ya modela
/// "usuario inactivo" como su propio estado (ver User.Deactivate /
/// EnsureCanAuthenticate), así que reutilizarlo aquí es la opción correcta,
/// no un atajo.
/// </summary>
public sealed record DeactivateUserCommand(string Id, string RequestedByUserId) : IRequest;
