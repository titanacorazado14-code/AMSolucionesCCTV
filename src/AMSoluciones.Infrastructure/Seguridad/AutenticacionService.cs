using AMSoluciones.Application.Abstracciones;
using Microsoft.AspNetCore.Identity;

namespace AMSoluciones.Infrastructure.Seguridad;

/// <summary>Valida el correo y la contraseña con ASP.NET Core Identity.</summary>
public class AutenticacionService : IAutenticacionService
{
    private readonly UserManager<IdentityUser> _usuarios;

    public AutenticacionService(UserManager<IdentityUser> usuarios)
    {
        _usuarios = usuarios;
    }

    public async Task<UsuarioAutenticado?> ValidarCredencialesAsync(string email, string contrasena, CancellationToken ct = default)
    {
        var usuario = await _usuarios.FindByEmailAsync(email);
        if (usuario is null)
            return null;

        if (!await _usuarios.CheckPasswordAsync(usuario, contrasena))
            return null;

        var roles = await _usuarios.GetRolesAsync(usuario);
        return new UsuarioAutenticado(usuario.Id, usuario.UserName ?? email, roles.ToList());
    }
}
