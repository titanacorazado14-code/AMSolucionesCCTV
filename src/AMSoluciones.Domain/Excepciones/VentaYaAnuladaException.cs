namespace AMSoluciones.Domain.Excepciones;

/// <summary>
/// Se lanza al intentar anular una venta que ya está anulada.
/// </summary>
public class VentaYaAnuladaException : DominioException
{
    public int VentaId { get; }

    public VentaYaAnuladaException(int ventaId)
        : base($"La venta {ventaId} ya se encuentra anulada.")
    {
        VentaId = ventaId;
    }
}
