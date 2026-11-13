using AMSoluciones.Application.CasosDeUso;
using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Domain.Constantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMSoluciones.Api.Controllers;

[ApiController]
[Route("api/ventas")]
[Authorize(Roles = Roles.Ambos)]
public class VentasController : ControllerBase
{
    private readonly VentaService _consultas;
    private readonly RegistrarVentaUseCase _registrar;
    private readonly AnularVentaUseCase _anular;

    public VentasController(VentaService consultas, RegistrarVentaUseCase registrar, AnularVentaUseCase anular)
    {
        _consultas = consultas;
        _registrar = registrar;
        _anular = anular;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VentaDto>>> Listar(CancellationToken ct) =>
        Ok(await _consultas.ListarAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VentaDto>> Obtener(int id, CancellationToken ct) =>
        Ok(await _consultas.ObtenerAsync(id, ct));

    [HttpPost]
    public async Task<ActionResult<VentaDto>> Registrar(VentaInput input, CancellationToken ct)
    {
        var venta = await _registrar.EjecutarAsync(input, ct);
        return CreatedAtAction(nameof(Obtener), new { id = venta.Id }, venta);
    }

    /// <summary>Solo el Administrador puede anular. La solicitud no lleva cuerpo.</summary>
    [HttpPost("{id:int}/anulacion")]
    [Authorize(Roles = Roles.Administrador)]
    public async Task<ActionResult<VentaDto>> Anular(int id, CancellationToken ct) =>
        Ok(await _anular.EjecutarAsync(id, ct));
}
