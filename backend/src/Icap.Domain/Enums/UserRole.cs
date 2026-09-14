namespace Icap.Domain.Enums;

/// <summary>
/// Rol del usuario dentro de la plataforma ICAP Juvenil.
/// Admin: control total (gestión de delegados, reportes, cancelación de recibos).
/// Delegate: solo puede emitir recibos para su área/región.
/// </summary>
public enum UserRole
{
    Admin = 0,
    Delegate = 1
}
