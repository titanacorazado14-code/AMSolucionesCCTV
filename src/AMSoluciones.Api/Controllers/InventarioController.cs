using AMSoluciones.Application.CasosDeUso;
using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Domain.Constantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMSoluciones.Api.Controllers;

[ApiController]
[Route("api/inventario")]
[Authorize(Roles = Roles.Ambos)]
public class InventarioController : ControllerBase
{
    private readonly InventarioService _inventario;
    private readonly ReponerInventarioUseCase _reponer;

    public InventarioController(InventarioService inventario, ReponerInventarioUseCase reponer)
    {
        _inventario = inventario;
        _reponer = reponer;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExistenciaDto>>> Listar(CancellationToken ct) =>
        Ok(await _inventario.ListarAsync(ct));

    /// <summary>El id de la ruta identifica al producto.</summary>
    [HttpPost("{id:int}/reposiciones")]
    public async Task<ActionResult<ExistenciaDto>> Reponer(int id, ReposicionInput input, CancellationToken ct) =>
        Ok(await _reponer.EjecutarAsync(id, input, ct));
}
