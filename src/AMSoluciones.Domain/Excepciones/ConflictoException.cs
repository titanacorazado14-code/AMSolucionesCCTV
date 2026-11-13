namespace AMSoluciones.Domain.Excepciones;

/// <summary>
/// La operación choca con el estado actual de los datos. La API la traduce a HTTP 409.
/// </summary>
public class ConflictoException : DominioException
{
    public ConflictoException(string mensaje, string codigo = "CONFLICTO") : base(mensaje, codigo)
    {
    }
}
