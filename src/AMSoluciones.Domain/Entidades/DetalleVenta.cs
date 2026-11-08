using AMSoluciones.Domain.Comun;
using AMSoluciones.Domain.Excepciones;

namespace AMSoluciones.Domain.Entidades;

/// <summary>
/// Línea de una venta. Su constructor es "internal": solo la clase Venta
/// puede crear detalles. Eso es la composición Venta-DetalleVenta.
/// </summary>
public class DetalleVenta
{
    public int Id { get; private set; }
    public int VentaId { get; private set; }
    public int ProductoId { get; private set; }

    // Copias históricas: no cambian aunque el producto se edite después.
    public string ProductoNombre { get; private set; } = string.Empty;
    public decimal PrecioUnitario { get; private set; }

    public int Cantidad { get; private set; }
    public decimal Subtotal { get; private set; }

    // Constructor vacío solo para Entity Framework.
    private DetalleVenta()
    {
    }

    internal DetalleVenta(int productoId, string productoNombre, int cantidad, decimal precioUnitario)
    {
        if (productoId <= 0)
            throw new DominioException("El detalle debe referenciar un producto.");

        if (cantidad <= 0)
            throw new DominioException("La cantidad vendida debe ser mayor que cero.");

        if (precioUnitario <= 0)
            throw new DominioException("El precio unitario debe ser mayor que cero.");

        ProductoId = productoId;
        ProductoNombre = Validar.TextoObligatorio(productoNombre, "Nombre del producto", 120);
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        Subtotal = cantidad * precioUnitario;
    }
}
