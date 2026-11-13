namespace AMSoluciones.Domain.Excepciones;

/// <summary>
/// Se lanza cuando una reposición o devolución haría superar el máximo de existencias.
/// </summary>
public class LimiteInventarioException : ConflictoException
{
    public int ProductoId { get; }
    public int Maximo { get; }

    public LimiteInventarioException(int productoId, int disponible, int cantidad, int maximo)
        : base($"El producto {productoId} tiene {disponible} unidades; agregar {cantidad} superaría el máximo de {maximo}.",
               "STOCK_MAXIMO")
    {
        ProductoId = productoId;
        Maximo = maximo;
    }
}
