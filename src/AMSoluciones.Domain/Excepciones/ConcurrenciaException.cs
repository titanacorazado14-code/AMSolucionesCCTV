namespace AMSoluciones.Domain.Excepciones;

/// <summary>
/// Otra operación modificó los mismos datos al mismo tiempo (control optimista con rowversion).
/// Todos los cambios de la operación se revierten.
/// </summary>
public class ConcurrenciaException : ConflictoException
{
    public ConcurrenciaException()
        : base("Otra operación modificó las existencias o la venta al mismo tiempo. Intente de nuevo.",
               "CONFLICTO_STOCK")
    {
    }
}
