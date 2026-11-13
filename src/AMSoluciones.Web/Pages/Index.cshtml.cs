using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Web.Paginas;

namespace AMSoluciones.Web.Pages;

public class IndexModel : PaginaBase
{
    private readonly DashboardService _dashboard;
    private readonly InventarioService _inventario;

    public IndexModel(DashboardService dashboard, InventarioService inventario)
    {
        _dashboard = dashboard;
        _inventario = inventario;
    }

    public ResumenDto Resumen { get; private set; } = new(0, 0, 0, 0m);
    public IReadOnlyList<ExistenciaDto> PocasExistencias { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Resumen = await _dashboard.ResumenAsync(ct);

        var existencias = await _inventario.ListarAsync(ct);
        PocasExistencias = existencias
            .Where(e => e.Disponible <= 5)
            .OrderBy(e => e.Disponible)
            .ToList();
    }
}
