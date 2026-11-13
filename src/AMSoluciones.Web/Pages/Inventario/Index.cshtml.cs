using AMSoluciones.Application.CasosDeUso;
using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Web.Paginas;
using Microsoft.AspNetCore.Mvc;

namespace AMSoluciones.Web.Pages.Inventario;

public class IndexModel : PaginaBase
{
    private readonly InventarioService _inventario;
    private readonly ReponerInventarioUseCase _reponer;

    public IndexModel(InventarioService inventario, ReponerInventarioUseCase reponer)
    {
        _inventario = inventario;
        _reponer = reponer;
    }

    public IReadOnlyList<ExistenciaDto> Existencias { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        Existencias = await _inventario.ListarAsync(ct);
    }

    // Caso de uso: reponer inventario (permitido a Administrador y Gestor).
    public async Task<IActionResult> OnPostReponerAsync(int productoId, int cantidad, CancellationToken ct)
    {
        ExistenciaDto? resultado = null;

        await IntentarAsync(async () =>
        {
            resultado = await _reponer.EjecutarAsync(productoId, new ReposicionInput(cantidad), ct);
        }, "Reposición registrada.");

        if (resultado is not null)
            TempData["Exito"] = $"Reposición registrada: {resultado.ProductoNombre} ahora tiene {resultado.Disponible} unidades.";

        return RedirectToPage();
    }
}
