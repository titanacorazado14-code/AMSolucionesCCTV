using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Domain.Constantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMSoluciones.Api.Controllers;

[ApiController]
[Route("api/categorias")]
[Authorize(Roles = Roles.Ambos)]
public class CategoriasController : ControllerBase
{
    private readonly CategoriaService _servicio;

    public CategoriasController(CategoriaService servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaDto>>> Listar(CancellationToken ct) =>
        Ok(await _servicio.ListarAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await _servicio.ObtenerAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<CategoriaDto>> Crear(CategoriaInput input, CancellationToken ct)
    {
        var creada = await _servicio.CrearAsync(input, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creada.Id }, creada);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<CategoriaDto>> Actualizar(int id, CategoriaInput input, CancellationToken ct) =>
        Ok(await _servicio.ActualizarAsync(id, input, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _servicio.EliminarAsync(id, ct);
        return NoContent();
    }
}
