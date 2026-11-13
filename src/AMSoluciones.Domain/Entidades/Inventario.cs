using AMSoluciones.Domain.Excepciones;

namespace AMSoluciones.Domain.Entidades;

/// <summary>
/// Existencias de un producto (una fila por producto).
/// Todo cambio de stock pasa obligatoriamente por Reponer o Descontar.
/// </summary>
public class Inventario
{
    public const int Maximo = 1_000_000;

    public int ProductoId { get; private set; }
    public Producto? Producto { get; private set; }
    public int Disponible { get; private set; }

    // Columna rowversion: la base de datos detecta cambios simultáneos.
    public byte[] VersionFila { get; private set; } = [];

    // Constructor vacío solo para Entity Framework.
    private Inventario()
    {
    }

    public Inventario(int productoId, int disponible = 0)
    {
        if (productoId <= 0)
            throw new DominioException("El inventario debe pertenecer a un producto.", "PRODUCTO_OBLIGATORIO");

        if (disponible < 0 || disponible > Maximo)
            throw new DominioException($"Las existencias deben estar entre 0 y {Maximo}.", "STOCK_INVALIDO");

        ProductoId = productoId;
        Disponible = disponible;
    }

    /// <summary>Comprueba que haya unidades suficientes, sin modificar nada.</summary>
    public void VerificarDisponibilidad(int cantidad)
    {
        ValidarCantidad(cantidad);

        if (cantidad > Disponible)
            throw new StockInsuficienteException(ProductoId, Disponible, cantidad);
    }

    /// <summary>Comprueba que quepan más unidades sin superar el máximo, sin modificar nada.</summary>
    public void VerificarCapacidad(int cantidad)
    {
        ValidarCantidad(cantidad);

        if ((long)Disponible + cantidad > Maximo)
            throw new LimiteInventarioException(ProductoId, Disponible, cantidad, Maximo);
    }

    public void Reponer(int cantidad)
    {
        VerificarCapacidad(cantidad);
        Disponible += cantidad;
    }

    public void Descontar(int cantidad)
    {
        VerificarDisponibilidad(cantidad);
        Disponible -= cantidad;
    }

    private static void ValidarCantidad(int cantidad)
    {
        if (cantidad < 1 || cantidad > Maximo)
            throw new DominioException($"La cantidad debe estar entre 1 y {Maximo}.", "CANTIDAD_INVALIDA");
    }
}
