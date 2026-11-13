using AMSoluciones.Domain.Comun;
using AMSoluciones.Domain.Enums;
using AMSoluciones.Domain.Excepciones;

namespace AMSoluciones.Domain.Entidades;

/// <summary>
/// Raíz del agregado Venta. Controla sus detalles: nadie puede agregar
/// ni quitar líneas por fuera de esta clase.
/// </summary>
public class Venta
{
    public const int MaximoLineas = 50;

    private readonly List<DetalleVenta> _detalles = new();

    public int Id { get; private set; }
    public DateTimeOffset FechaUtc { get; private set; }
    public EstadoVenta Estado { get; private set; }
    public decimal Total { get; private set; }
    public DateTimeOffset? AnuladaUtc { get; private set; }
    public byte[] VersionFila { get; private set; } = [];

    // Solo lectura desde afuera.
    public IReadOnlyCollection<DetalleVenta> Detalles => _detalles.AsReadOnly();

    // Constructor vacío solo para Entity Framework.
    private Venta()
    {
    }

    /// <param name="fechaUtc">Normalmente se omite (se usa la hora actual). Se acepta para cargar datos históricos.</param>
    public Venta(DateTimeOffset? fechaUtc = null)
    {
        FechaUtc = Reloj.Truncar(fechaUtc ?? DateTimeOffset.UtcNow);
        Estado = EstadoVenta.Confirmada;
    }

    public void AgregarDetalle(int productoId, string productoNombre, int cantidad, decimal precioUnitario)
    {
        if (Estado == EstadoVenta.Anulada)
            throw new DominioException("No se pueden agregar líneas a una venta anulada.", "VENTA_ANULADA");

        if (_detalles.Count >= MaximoLineas)
            throw new DominioException($"Una venta admite como máximo {MaximoLineas} líneas.", "DEMASIADAS_LINEAS");

        if (_detalles.Any(d => d.ProductoId == productoId))
            throw new DominioException("La venta no admite productos repetidos.", "LINEA_REPETIDA");

        _detalles.Add(new DetalleVenta(productoId, productoNombre, cantidad, precioUnitario));
        RecalcularTotal();
    }

    /// <summary>Verifica que la venta pueda registrarse: debe tener al menos una línea.</summary>
    public void Confirmar()
    {
        if (_detalles.Count == 0)
            throw new DominioException("La venta debe tener al menos una línea.", "VENTA_VACIA");
    }

    public void Anular(DateTimeOffset? fechaUtc = null)
    {
        if (Estado == EstadoVenta.Anulada)
            throw new VentaYaAnuladaException(Id);

        var fecha = Reloj.Truncar(fechaUtc ?? DateTimeOffset.UtcNow);
        if (fecha < FechaUtc)
            throw new DominioException("La fecha de anulación no puede ser anterior a la venta.", "FECHA_INVALIDA");

        Estado = EstadoVenta.Anulada;
        AnuladaUtc = fecha;
    }

    private void RecalcularTotal()
    {
        Total = _detalles.Sum(d => d.Subtotal);
    }
}
