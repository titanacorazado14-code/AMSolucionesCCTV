using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace AMSoluciones.Infrastructure.Persistencia.Repositorios;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly AppDbContext _db;

    public CategoriaRepository(AppDbContext db) => _db = db;

    public Task<Categoria?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Categorias.FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken ct = default) =>
        await _db.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync(ct);

    public Task<bool> ExistePorNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default)
    {
        var limpio = nombre.Trim();
        // La comparación usa el collation de la columna (sin distinguir mayúsculas).
        return _db.Categorias.AnyAsync(c => c.Nombre == limpio && (excluirId == null || c.Id != excluirId), ct);
    }

    public Task<bool> TieneProductosAsync(int categoriaId, CancellationToken ct = default) =>
        _db.Productos.AnyAsync(p => p.CategoriaId == categoriaId, ct);

    public async Task AgregarAsync(Categoria categoria, CancellationToken ct = default) =>
        await _db.Categorias.AddAsync(categoria, ct);

    public void Eliminar(Categoria categoria) => _db.Categorias.Remove(categoria);
}
