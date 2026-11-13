using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Web.Paginas;
using Microsoft.AspNetCore.Mvc;

namespace AMSoluciones.Web.Pages.Categorias;

public class IndexModel : PaginaBase
{
    private readonly CategoriaService _servicio;

    public IndexModel(CategoriaService servicio)
    {
        _servicio = servicio;
    }

    public IReadOnlyList<CategoriaDto> Categorias { get; private set; } = [];

    // Campos del formulario (crear o editar).
    [BindProperty] public int? Id { get; set; }
    [BindProperty] public string Nombre { get; set; } = string.Empty;

    public async Task OnGetAsync(int? editar, CancellationToken ct)
    {
        Categorias = await _servicio.ListarAsync(ct);

        if (editar is int idEditar && EsAdministrador)
        {
            var categoria = Categorias.FirstOrDefault(c => c.Id == idEditar);
            if (categoria is not null)
            {
                Id = categoria.Id;
                Nombre = categoria.Nombre;
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
            if (Id is int id)
                await _servicio.ActualizarAsync(id, new CategoriaInput(Nombre), ct);
            else
                await _servicio.CrearAsync(new CategoriaInput(Nombre), ct);
        }, editando ? "Categoría actualizada." : "Categoría creada.");

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id, CancellationToken ct)
    {
        if (!EsAdministrador)
            return Forbid();

        await IntentarAsync(() => _servicio.EliminarAsync(id, ct), "Categoría eliminada.");
        return RedirectToPage();
    }
}
