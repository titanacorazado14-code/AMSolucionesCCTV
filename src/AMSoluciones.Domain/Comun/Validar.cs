using AMSoluciones.Domain.Excepciones;

namespace AMSoluciones.Domain.Comun;

/// <summary>
/// Validaciones de texto reutilizadas por las entidades.
/// Es "internal": solo la capa Domain puede usarla.
/// </summary>
internal static class Validar
{
    public static string TextoObligatorio(string? valor, string campo, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DominioException($"El campo '{campo}' es obligatorio.");

        var limpio = valor.Trim();
        if (limpio.Length > maximo)
            throw new DominioException($"El campo '{campo}' no puede superar {maximo} caracteres.");

        return limpio;
    }

    public static string? TextoOpcional(string? valor, string campo, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        var limpio = valor.Trim();
        if (limpio.Length > maximo)
            throw new DominioException($"El campo '{campo}' no puede superar {maximo} caracteres.");

        return limpio;
    }
}
