namespace AMSoluciones.Domain.Constantes;

/// <summary>
/// Nombres de los roles del sistema.
/// </summary>
public static class Roles
{
    public const string Administrador = "Administrador";
    public const string Gestor = "Gestor";

    /// <summary>Ambos roles, para [Authorize(Roles = Roles.Ambos)].</summary>
    public const string Ambos = Administrador + "," + Gestor;
}
