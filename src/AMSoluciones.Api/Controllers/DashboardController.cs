using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Domain.Constantes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMSoluciones.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize(Roles = Roles.Ambos)]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _servicio;

    public DashboardController(DashboardService servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<ActionResult<ResumenDto>> Resumen(CancellationToken ct) =>
        Ok(await _servicio.ResumenAsync(ct));
}
