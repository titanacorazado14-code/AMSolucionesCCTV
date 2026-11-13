namespace AMSoluciones.Application.Abstracciones;

public sealed record UsuarioAutenticado(string Id, string Nombre, IReadOnlyList<string> Roles);

/// <summary>
/// Valida credenciales. La implementa Infrastructure (con ASP.NET Core Identity);
/// la usan la API (para emitir el JWT) y la Web (para la cookie de sesión).
/// </summary>
public interface IAutenticacionService
{
    /// <returns>El usuario con sus roles, o null si las credenciales son incorrectas.</returns>
    Task<UsuarioAutenticado?> ValidarCredencialesAsync(string email, string contrasena, CancellationToken ct = default);
}
