using AMSoluciones.Domain.Excepciones;

namespace AMSoluciones.Domain.Entidades;

/// <summary>
/// Existencias de un producto físico (una fila por producto).
/// Todo cambio de stock pasa obligatoriamente por Reponer o Descontar.
/// </summary>
public class Inventario
{
    public const int StockMaximo = 1_000_000;

    public int ProductoId { get; private set; }
    public Producto? Producto { get; private set; }
    public int Disponible { get; private set; }
    public int StockMinimo { get; private set; }
    public DateTime FechaActualizacionUtc { get; private set; }

    // Columna rowversion: la base de datos detecta cambios simultáneos.
    public byte[] VersionFila { get; private set; } = [];

    public bool EstaBajoMinimo => Disponible <= StockMinimo;

    // Constructor vacío solo para Entity Framework.
    private Inventario()
    {
    }

    public Inventario(int productoId, int disponible = 0, int stockMinimo = 0)
    {
        if (productoId <= 0)
            throw new DominioException("El inventario debe pertenecer a un producto.");

        if (disponible < 0 || disponible > StockMaximo)
            throw new DominioException($"El stock inicial debe estar entre 0 y {StockMaximo}.");

        if (stockMinimo < 0)
            throw new DominioException("El stock mínimo no puede ser negativo.");

        ProductoId = productoId;
        Disponible = disponible;
        StockMinimo = stockMinimo;
        FechaActualizacionUtc = DateTime.UtcNow;
    }

    public void Reponer(int cantidad)
    {
        if (cantidad <= 0)
            throw new DominioException("La cantidad a reponer debe ser mayor que cero.");

        if ((long)Disponible + cantidad > StockMaximo)
            throw new DominioException($"El stock no puede superar {StockMaximo} unidades.");

        Disponible += cantidad;
        FechaActualizacionUtc = DateTime.UtcNow;
    }

    public void Descontar(int cantidad)
    {
        if (cantidad <= 0)
            throw new DominioException("La cantidad a descontar debe ser mayor que cero.");

        if (cantidad > Disponible)
            throw new StockInsuficienteException(ProductoId, Disponible, cantidad);

        Disponible -= cantidad;
        FechaActualizacionUtc = DateTime.UtcNow;
    }

    public void CambiarStockMinimo(int stockMinimo)
    {
        if (stockMinimo < 0)
            throw new DominioException("El stock mínimo no puede ser negativo.");

        StockMinimo = stockMinimo;
        FechaActualizacionUtc = DateTime.UtcNow;
    }
}
