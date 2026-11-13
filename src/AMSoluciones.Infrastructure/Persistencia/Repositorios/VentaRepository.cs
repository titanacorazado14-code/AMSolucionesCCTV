using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Repositorios;
using Microsoft.EntityFrameworkCore;

namespace AMSoluciones.Infrastructure.Persistencia.Repositorios;

public class VentaRepository : IVentaRepository
{
    private readonly AppDbContext _db;

    public VentaRepository(AppDbContext db) => _db = db;

    public Task<Venta?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Ventas.Include(v => v.Detalles).FirstOrDefaultAsync(v => v.Id == id, ct);

    public async Task<IReadOnlyList<Venta>> ListarAsync(CancellationToken ct = default) =>
        await _db.Ventas
            .AsNoTracking()
            .Include(v => v.Detalles)
            .OrderByDescending(v => v.FechaUtc)
            .ThenByDescending(v => v.Id)
            .ToListAsync(ct);

    public async Task AgregarAsync(Venta venta, CancellationToken ct = default) =>
        await _db.Ventas.AddAsync(venta, ct);
}
