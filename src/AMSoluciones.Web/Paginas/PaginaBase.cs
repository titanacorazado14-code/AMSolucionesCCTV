using AMSoluciones.Domain.Constantes;
using AMSoluciones.Domain.Excepciones;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AMSoluciones.Web.Paginas;

/// <summary>
/// Base de las páginas: sabe si el usuario es Administrador y convierte las
/// excepciones de negocio en un mensaje visible (en vez de una pantalla de error).
/// </summary>
public abstract class PaginaBase : PageModel
{
    public bool EsAdministrador => User.IsInRole(Roles.Administrador);

    protected async Task<bool> IntentarAsync(Func<Task> accion, string mensajeExito)
    {
        try
        {
            await accion();
            TempData["Exito"] = mensajeExito;
            return true;
        }
        catch (DominioException ex)
        {
            TempData["Error"] = ex.Message;
            return false;
        }
    }
}
