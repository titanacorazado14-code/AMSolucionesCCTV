using AMSoluciones.Domain.Entidades;

namespace AMSoluciones.Domain.Repositorios;

public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Producto>> ObtenerPorIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Producto>> ListarAsync(CancellationToken ct = default);
    Task<bool> ExistePorSkuAsync(string sku, CancellationToken ct = default);
    Task<bool> TieneHistorialAsync(int productoId, CancellationToken ct = default);
    Task AgregarAsync(Producto producto, CancellationToken ct = default);
    void Eliminar(Producto producto);
}
