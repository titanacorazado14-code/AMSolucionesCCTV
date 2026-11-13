namespace AMSoluciones.Domain.Repositorios;

/// <summary>
/// Unidad de trabajo: guarda en una sola transacción todos los cambios hechos por los repositorios.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Guarda todo o nada. Lanza ConcurrenciaException si otra operación modificó los mismos datos.</summary>
    Task<int> GuardarCambiosAsync(CancellationToken ct = default);

    /// <summary>Ejecuta varios guardados dentro de una misma transacción explícita.</summary>
    Task EjecutarEnTransaccionAsync(Func<Task> operacion, CancellationToken ct = default);
}
