using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Web.Paginas;
using Microsoft.AspNetCore.Mvc;

namespace AMSoluciones.Web.Pages.Productos;

public class IndexModel : PaginaBase
{
    private readonly ProductoService _productos;
    private readonly CategoriaService _categorias;

    public IndexModel(ProductoService productos, CategoriaService categorias)
    {
        _productos = productos;
        _categorias = categorias;
    }

    public IReadOnlyList<ProductoDto> Productos { get; private set; } = [];
    public IReadOnlyList<CategoriaDto> Categorias { get; private set; } = [];

    // Campos del formulario (crear o editar).
    [BindProperty] public int? Id { get; set; }
    [BindProperty] public int CategoriaId { get; set; }
    [BindProperty] public string Sku { get; set; } = string.Empty;
    [BindProperty] public string Nombre { get; set; } = string.Empty;
    [BindProperty] public decimal Precio { get; set; }

    public string NombreCategoria(int categoriaId) =>
        Categorias.FirstOrDefault(c => c.Id == categoriaId)?.Nombre ?? "-";

    public async Task OnGetAsync(int? editar, CancellationToken ct)
    {
        await CargarAsync(ct);

        if (editar is int idEditar && EsAdministrador)
        {
            var producto = Productos.FirstOrDefault(p => p.Id == idEditar);
            if (producto is not null)
            {
                Id = producto.Id;
                CategoriaId = producto.CategoriaId;
                Sku = producto.Sku;
                Nombre = producto.Nombre;
                Precio = producto.Precio;
            }
        }
    }

    public async Task<IActionResult> OnPostGuardarAsync(CancellationToken ct)
    {
        if (!EsAdministrador)
            return Forbid();

        var editando = Id is not null;
        await IntentarAsync(async () =>
        {
            var entrada = new ProductoInput(CategoriaId, Sku, Nombre, Precio);

            if (Id is int id)
                await _productos.ActualizarAsync(id, entrada, ct);
            else
                await _productos.CrearAsync(entrada, ct);
        }, editando ? "Producto actualizado." : "Producto creado con su inventario en cero.");

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id, CancellationToken ct)
    {
        if (!EsAdministrador)
            return Forbid();

        await IntentarAsync(() => _productos.EliminarAsync(id, ct), "Producto eliminado.");
        return RedirectToPage();
    }

    private async Task CargarAsync(CancellationToken ct)
    {
        Productos = await _productos.ListarAsync(ct);
        Categorias = await _categorias.ListarAsync(ct);
    }
}
