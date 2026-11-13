using AMSoluciones.Api.Seguridad;
using AMSoluciones.Application.Abstracciones;
using AMSoluciones.Application.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMSoluciones.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAutenticacionService _autenticacion;
    private readonly JwtService _jwt;

    public AuthController(IAutenticacionService autenticacion, JwtService jwt)
    {
        _autenticacion = autenticacion;
        _jwt = jwt;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<SesionDto>> Login(LoginInput input, CancellationToken ct)
    {
        var usuario = await _autenticacion.ValidarCredencialesAsync(input.Email, input.Contrasena, ct);

        if (usuario is null)
            return Unauthorized();

        return Ok(_jwt.CrearSesion(usuario));
    }
}
