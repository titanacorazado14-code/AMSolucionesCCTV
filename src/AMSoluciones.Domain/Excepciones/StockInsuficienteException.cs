namespace AMSoluciones.Domain.Excepciones;

/// <summary>
/// Se lanza al intentar vender o descontar más unidades de las disponibles.
/// </summary>
public class StockInsuficienteException : ConflictoException
{
    public int ProductoId { get; }
    public int Disponible { get; }
    public int Solicitado { get; }

    public StockInsuficienteException(int productoId, int disponible, int solicitado)
        : base($"Existencias insuficientes para el producto {productoId}: disponible {disponible}, solicitado {solicitado}.",
               "STOCK_INSUFICIENTE")
    {
        ProductoId = productoId;
        Disponible = disponible;
        Solicitado = solicitado;
    }
}
