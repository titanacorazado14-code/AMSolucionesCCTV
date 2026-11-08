using AMSoluciones.Domain.Entidades;

namespace AMSoluciones.Domain.Repositorios;

public interface IMovimientoInventarioRepository
{
    Task<IReadOnlyList<MovimientoInventario>> ListarPorProductoAsync(int productoId, CancellationToken ct = default);
    Task AgregarAsync(MovimientoInventario movimiento, CancellationToken ct = default);
}
