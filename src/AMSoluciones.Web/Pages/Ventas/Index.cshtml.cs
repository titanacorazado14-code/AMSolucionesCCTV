using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Web.Paginas;

namespace AMSoluciones.Web.Pages.Ventas;

public class IndexModel : PaginaBase
{
    private readonly VentaService _servicio;

    public IndexModel(VentaService servicio)
    {
        _servicio = servicio;
    }

    public IReadOnlyList<VentaDto> Ventas { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Ventas = await _servicio.ListarAsync(ct);
    }
}
