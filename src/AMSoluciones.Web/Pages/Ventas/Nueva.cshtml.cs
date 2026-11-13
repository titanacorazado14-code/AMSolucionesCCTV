using AMSoluciones.Application.CasosDeUso;
using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Web.Paginas;
using Microsoft.AspNetCore.Mvc;

namespace AMSoluciones.Web.Pages.Ventas;

public class NuevaModel : PaginaBase
{
    private readonly ProductoService _productos;
    private readonly InventarioService _inventario;
    private readonly RegistrarVentaUseCase _registrar;

    public NuevaModel(ProductoService productos, InventarioService inventario, RegistrarVentaUseCase registrar)
    {
        _productos = productos;
        _inventario = inventario;
        _registrar = registrar;
    }

    public sealed record FilaProducto(int ProductoId, string Sku, string Nombre, decimal Precio, int Disponible);

    public sealed class LineaFormulario
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public List<FilaProducto> Filas { get; private set; } = new();

    [BindProperty]
    public List<LineaFormulario> Lineas { get; set; } = new();

    public int CantidadPrevia(int productoId) =>
        Lineas.FirstOrDefault(l => l.ProductoId == productoId)?.Cantidad ?? 0;

    public async Task OnGetAsync(CancellationToken ct)
    {
        await CargarAsync(ct);
    }

    // Caso de uso: registrar venta (permitido a Administrador y Gestor).
    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var lineas = Lineas
            .Where(l => l.Cantidad > 0)
            .Select(l => new LineaVentaInput(l.ProductoId, l.Cantidad))
            .ToList();

        if (lineas.Count == 0)
        {
            TempData["Error"] = "Indique la cantidad de al menos un producto.";
            await CargarAsync(ct);
            return Page();
        }

        VentaDto? venta = null;
        var registrada = await IntentarAsync(async () =>
        {
            venta = await _registrar.EjecutarAsync(new VentaInput(lineas), ct);
        }, "Venta registrada correctamente.");

        if (registrada && venta is not null)
            return RedirectToPage("Detalle", new { id = venta.Id });

        // Hubo un error de negocio (por ejemplo, stock insuficiente): se vuelve al formulario.
        await CargarAsync(ct);
        return Page();
    }

    private async Task CargarAsync(CancellationToken ct)
    {
        var productos = await _productos.ListarAsync(ct);
        var existencias = (await _inventario.ListarAsync(ct)).ToDictionary(x => x.ProductoId);

        Filas = productos
            .Select(p => new FilaProducto(
                p.Id,
                p.Sku,
                p.Nombre,
                p.Precio,
                existencias.TryGetValue(p.Id, out var existencia) ? existencia.Disponible : 0))
            .ToList();
    }
}
