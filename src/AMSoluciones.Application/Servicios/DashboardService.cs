using AMSoluciones.Application.Dtos;
using AMSoluciones.Domain.Enums;
using AMSoluciones.Domain.Repositorios;

namespace AMSoluciones.Application.Servicios;

/// <summary>Indicadores de la pantalla de inicio.</summary>
public class DashboardService
{
    private readonly IProductoRepository _productos;
    private readonly IInventarioRepository _inventarios;
    private readonly IVentaRepository _ventas;

    public DashboardService(IProductoRepository productos, IInventarioRepository inventarios, IVentaRepository ventas)
    {
        _productos = productos;
        _inventarios = inventarios;
        _ventas = ventas;
    }

    public async Task<ResumenDto> ResumenAsync(CancellationToken ct = default)
    {
        var productos = await _productos.ListarAsync(ct);
        var inventarios = await _inventarios.ListarAsync(ct);
        var ventas = await _ventas.ListarAsync(ct);

        var confirmadas = ventas.Where(v => v.Estado == EstadoVenta.Confirmada).ToList();

        return new ResumenDto(
            productos.Count,
            inventarios.Sum(i => (long)i.Disponible),
            confirmadas.Count,
            confirmadas.Sum(v => v.Total));
    }
}
