using AMSoluciones.Domain.Entidades;

namespace AMSoluciones.Domain.Repositorios;

public interface IProductoRepository
{
    Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Producto>> ObtenerPorIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Producto>> ListarAsync(CancellationToken ct = default);
    Task<bool> ExistePorSkuAsync(string sku, int? excluirId = null, CancellationToken ct = default);

    /// <summary>Indica si el producto aparece en algún detalle de venta (incluso de ventas anuladas).</summary>
    Task<bool> TieneDetallesAsync(int productoId, CancellationToken ct = default);

    Task AgregarAsync(Producto producto, CancellationToken ct = default);
    void Eliminar(Producto producto);
}
