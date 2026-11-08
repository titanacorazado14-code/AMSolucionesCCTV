namespace AMSoluciones.Domain.Repositorios;

/// <summary>
/// Guarda en una sola transacción todos los cambios hechos por los repositorios.
/// </summary>
public interface IUnitOfWork
{
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);
}
