using AMSoluciones.Domain.Enums;
using AMSoluciones.Domain.Excepciones;

namespace AMSoluciones.Domain.Entidades;

/// <summary>
/// Raíz del agregado Venta. Controla sus detalles: nadie puede agregar
/// ni quitar líneas por fuera de esta clase.
/// </summary>
public class Venta
{
    private readonly List<DetalleVenta> _detalles = new();

    public int Id { get; private set; }
    public int? ClienteId { get; private set; }   // null = venta de mostrador
    public Cliente? Cliente { get; private set; }
    public DateTime FechaUtc { get; private set; }
    public EstadoVenta Estado { get; private set; }
    public decimal Total { get; private set; }
    public DateTime? AnuladaUtc { get; private set; }
    public byte[] VersionFila { get; private set; } = [];

    // Solo lectura desde afuera.
    public IReadOnlyCollection<DetalleVenta> Detalles => _detalles.AsReadOnly();

    // Constructor vacío solo para Entity Framework.
    private Venta()
    {
    }

    public Venta(int? clienteId = null)
    {
        ClienteId = clienteId;
        FechaUtc = DateTime.UtcNow;
        Estado = EstadoVenta.Completada;
    }

    public void AgregarDetalle(int productoId, string productoNombre, int cantidad, decimal precioUnitario)
    {
        if (Estado == EstadoVenta.Anulada)
            throw new DominioException("No se pueden agregar detalles a una venta anulada.");

        if (_detalles.Any(d => d.ProductoId == productoId))
            throw new DominioException("El producto ya está incluido en la venta.");

        _detalles.Add(new DetalleVenta(productoId, productoNombre, cantidad, precioUnitario));
        RecalcularTotal();
    }

    /// <summary>
    /// Verifica que la venta pueda registrarse: debe tener al menos un detalle.
    /// </summary>
    public void Confirmar()
    {
        if (_detalles.Count == 0)
            throw new DominioException("La venta debe tener al menos un detalle.");
    }

    public void Anular()
    {
        if (Estado == EstadoVenta.Anulada)
            throw new VentaYaAnuladaException(Id);

        Estado = EstadoVenta.Anulada;
        AnuladaUtc = DateTime.UtcNow;
    }

    private void RecalcularTotal()
    {
        Total = _detalles.Sum(d => d.Subtotal);
    }
}
