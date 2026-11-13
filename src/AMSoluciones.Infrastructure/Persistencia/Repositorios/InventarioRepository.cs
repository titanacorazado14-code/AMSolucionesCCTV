using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace AMSoluciones.Infrastructure.Persistencia.Repositorios;

public class InventarioRepository : IInventarioRepository
{
    private readonly AppDbContext _db;

    public InventarioRepository(AppDbContext db) => _db = db;

    public Task<Inventario?> ObtenerPorProductoIdAsync(int productoId, CancellationToken ct = default) =>
        _db.Inventarios.FirstOrDefaultAsync(i => i.ProductoId == productoId, ct);

    public async Task<IReadOnlyList<Inventario>> ObtenerPorProductoIdsAsync(IEnumerable<int> productoIds, CancellationToken ct = default)
    {
        var lista = productoIds.Distinct().ToList();
        return await _db.Inventarios.Where(i => lista.Contains(i.ProductoId)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Inventario>> ListarAsync(CancellationToken ct = default) =>
        await _db.Inventarios.AsNoTracking().ToListAsync(ct);

    public async Task AgregarAsync(Inventario inventario, CancellationToken ct = default) =>
        await _db.Inventarios.AddAsync(inventario, ct);

    public void Eliminar(Inventario inventario) => _db.Inventarios.Remove(inventario);
}
