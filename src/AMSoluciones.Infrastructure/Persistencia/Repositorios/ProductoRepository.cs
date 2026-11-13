using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace AMSoluciones.Infrastructure.Persistencia.Repositorios;

public class ProductoRepository : IProductoRepository
{
    private readonly AppDbContext _db;

    public ProductoRepository(AppDbContext db) => _db = db;

    public Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Productos.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Producto>> ObtenerPorIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var lista = ids.Distinct().ToList();
        return await _db.Productos.Where(p => lista.Contains(p.Id)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Producto>> ListarAsync(CancellationToken ct = default) =>
        await _db.Productos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync(ct);

    public Task<bool> ExistePorSkuAsync(string sku, int? excluirId = null, CancellationToken ct = default)
    {
        var normalizado = sku.Trim().ToUpperInvariant();
        return _db.Productos.AnyAsync(p => p.Sku == normalizado && (excluirId == null || p.Id != excluirId), ct);
    }

    public Task<bool> TieneDetallesAsync(int productoId, CancellationToken ct = default) =>
        _db.DetallesVenta.AnyAsync(d => d.ProductoId == productoId, ct);

    public async Task AgregarAsync(Producto producto, CancellationToken ct = default) =>
        await _db.Productos.AddAsync(producto, ct);

    public void Eliminar(Producto producto) => _db.Productos.Remove(producto);
}
