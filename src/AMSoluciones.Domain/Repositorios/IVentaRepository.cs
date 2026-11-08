using AMSoluciones.Domain.Entidades;

namespace AMSoluciones.Domain.Repositorios;

public interface IVentaRepository
{
    /// <summary>Devuelve la venta junto con sus detalles.</summary>
    Task<Venta?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Venta>> ListarAsync(CancellationToken ct = default);
    Task AgregarAsync(Venta venta, CancellationToken ct = default);
}
