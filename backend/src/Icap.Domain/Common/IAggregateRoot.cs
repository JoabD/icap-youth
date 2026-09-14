namespace Icap.Domain.Common;

/// <summary>
/// Marca una Entity como raíz de un Aggregate (única puerta de entrada
/// transaccional/consistencia para su sub-árbol de objetos). Solo los
/// Aggregate Roots tienen Repository propio (User, Receipt).
/// </summary>
public interface IAggregateRoot
{
}
