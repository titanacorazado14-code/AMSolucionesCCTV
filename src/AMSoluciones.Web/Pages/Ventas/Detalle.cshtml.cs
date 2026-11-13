using AMSoluciones.Application.CasosDeUso;
using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Domain.Excepciones;
using AMSoluciones.Web.Paginas;
using Microsoft.AspNetCore.Mvc;

namespace AMSoluciones.Web.Pages.Ventas;

public class DetalleModel : PaginaBase
{
    private readonly VentaService _ventas;
    private readonly AnularVentaUseCase _anular;

    public DetalleModel(VentaService ventas, AnularVentaUseCase anular)
    {
        _ventas = ventas;
        _anular = anular;
    }

    public VentaDto Venta { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        try
        {
            Venta = await _ventas.ObtenerAsync(id, ct);
        }
        catch (NoEncontradoException)
        {
            return NotFound();
        }

        return Page();
    }

    // Caso de uso: anular venta (solo Administrador).
    public async Task<IActionResult> OnPostAnularAsync(int id, CancellationToken ct)
    {
        if (!EsAdministrador)
            return Forbid();

        await IntentarAsync(() => _anular.EjecutarAsync(id, ct), "Venta anulada. Las unidades volvieron al inventario.");
        return RedirectToPage(new { id });
    }
}
