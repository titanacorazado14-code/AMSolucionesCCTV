using AMSoluciones.Domain.Comun;
using AMSoluciones.Domain.Enums;
using AMSoluciones.Domain.Excepciones;

namespace AMSoluciones.Domain.Entidades;

/// <summary>
/// Registro histórico de cada entrada, salida o anulación de stock.
/// </summary>
public class MovimientoInventario
{
    public long Id { get; private set; }
    public int ProductoId { get; private set; }
    public TipoMovimiento Tipo { get; private set; }
    public int Cantidad { get; private set; }
    public DateTime FechaUtc { get; private set; }
    public string? Referencia { get; private set; }
    public int? VentaId { get; private set; }

    // Constructor vacío solo para Entity Framework.
    private MovimientoInventario()
    {
    }

    public MovimientoInventario(int productoId, TipoMovimiento tipo, int cantidad,
        string? referencia = null, int? ventaId = null)
    {
        if (productoId <= 0)
            throw new DominioException("El movimiento debe referenciar un producto.");

        if (cantidad <= 0)
            throw new DominioException("La cantidad del movimiento debe ser mayor que cero.");

        ProductoId = productoId;
        Tipo = tipo;
        Cantidad = cantidad;
        Referencia = Validar.TextoOpcional(referencia, "Referencia", 100);
        VentaId = ventaId;
        FechaUtc = DateTime.UtcNow;
    }
}
