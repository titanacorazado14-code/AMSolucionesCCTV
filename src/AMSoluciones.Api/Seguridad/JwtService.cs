using System.Security.Claims;
using System.Text;
using AMSoluciones.Application.Abstracciones;
using AMSoluciones.Application.Dtos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AMSoluciones.Api.Seguridad;

/// <summary>Crea el token JWT que el cliente enviará en el encabezado Authorization.</summary>
public class JwtService
{
    private readonly JwtOpciones _opciones;

    public JwtService(IOptions<JwtOpciones> opciones)
    {
        _opciones = opciones.Value;
    }

    public SesionDto CrearSesion(UsuarioAutenticado usuario)
    {
        var claims = new List<Claim>
        {
            new("sub", usuario.Id),
            new("name", usuario.Nombre)
        };
        claims.AddRange(usuario.Roles.Select(rol => new Claim("role", rol)));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _opciones.Issuer,
            Audience = _opciones.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_opciones.MinutosExpiracion),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new SesionDto(token, usuario.Nombre, usuario.Roles.ToArray());
    }
}
