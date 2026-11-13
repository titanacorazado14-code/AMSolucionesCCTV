using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Domain.Constantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMSoluciones.Api.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize(Roles = Roles.Ambos)]
public class ProductosController : ControllerBase
{
    private readonly ProductoService _servicio;

    public ProductosController(ProductoService servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductoDto>>> Listar(CancellationToken ct) =>
        Ok(await _servicio.ListarAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await _servicio.ObtenerAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<ProductoDto>> Crear(ProductoInput input, CancellationToken ct)
    {
        var creado = await _servicio.CrearAsync(input, ct);
        return CreatedAtAction(nameof(Obtener), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<ProductoDto>> Actualizar(int id, ProductoInput input, CancellationToken ct) =>
        Ok(await _servicio.ActualizarAsync(id, input, ct));

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
    {
        await _servicio.EliminarAsync(id, ct);
        return NoContent();
    }
}
