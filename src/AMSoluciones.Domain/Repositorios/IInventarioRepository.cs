using AMSoluciones.Domain.Entidades;

namespace AMSoluciones.Domain.Repositorios;

public interface IInventarioRepository
{
    Task<Inventario?> ObtenerPorProductoIdAsync(int productoId, CancellationToken ct = default);
    Task<IReadOnlyList<Inventario>> ObtenerPorProductoIdsAsync(IEnumerable<int> productoIds, CancellationToken ct = default);
    Task<IReadOnlyList<Inventario>> ListarAsync(CancellationToken ct = default);
    Task AgregarAsync(Inventario inventario, CancellationToken ct = default);
}
