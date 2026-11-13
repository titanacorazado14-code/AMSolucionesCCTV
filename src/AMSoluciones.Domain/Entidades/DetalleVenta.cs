using AMSoluciones.Domain.Comun;
using AMSoluciones.Domain.Excepciones;

namespace AMSoluciones.Domain.Entidades;

/// <summary>
/// Línea de una venta. Su constructor es "internal": solo la clase Venta
/// puede crear detalles. Eso representa la composición Venta-DetalleVenta.
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

    // En SQL Server es una columna calculada; aquí se calcula igual para trabajar en memoria.
    public decimal Subtotal { get; private set; }

    // Constructor vacío solo para Entity Framework.
    private DetalleVenta()
    {
    }

    internal DetalleVenta(int productoId, string productoNombre, int cantidad, decimal precioUnitario)
    {
        if (productoId <= 0)
            throw new DominioException("La línea debe referenciar un producto.", "PRODUCTO_OBLIGATORIO");

        if (cantidad < 1 || cantidad > Inventario.Maximo)
            throw new DominioException($"La cantidad debe estar entre 1 y {Inventario.Maximo}.", "CANTIDAD_INVALIDA");

        if (precioUnitario <= 0)
            throw new DominioException("El precio unitario debe ser mayor que cero.", "PRECIO_INVALIDO");

        ProductoId = productoId;
        ProductoNombre = Validar.TextoObligatorio(productoNombre, "Nombre del producto", Producto.LongitudMaximaNombre);
        Cantidad = cantidad;
        PrecioUnitario = precioUnitario;
        Subtotal = cantidad * precioUnitario;
    }
}
