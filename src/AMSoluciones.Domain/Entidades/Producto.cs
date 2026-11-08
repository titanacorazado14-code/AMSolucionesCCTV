using System.Text.RegularExpressions;
using AMSoluciones.Domain.Comun;
using AMSoluciones.Domain.Excepciones;

namespace AMSoluciones.Domain.Entidades;

public class Producto
{
    public const int LongitudMaximaSku = 32;
    public const int LongitudMaximaNombre = 120;

    // Solo letras mayúsculas, números y guiones.
    private static readonly Regex PatronSku = new("^[A-Z0-9-]+$", RegexOptions.Compiled);

    public int Id { get; private set; }
    public int CategoriaId { get; private set; }
    public Categoria? Categoria { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string? Marca { get; private set; }
    public string? Modelo { get; private set; }
    public string? Descripcion { get; private set; }
    public decimal PrecioCompra { get; private set; }
    public decimal PrecioVenta { get; private set; }
    public bool EsServicio { get; private set; }
    public bool Activo { get; private set; } = true;
    public DateTime FechaCreacionUtc { get; private set; }

    // Constructor vacío solo para Entity Framework.
    private Producto()
    {
    }

    public Producto(
        int categoriaId,
        string sku,
        string nombre,
        decimal precioCompra,
        decimal precioVenta,
        bool esServicio = false,
        string? marca = null,
        string? modelo = null,
        string? descripcion = null)
    {
        if (categoriaId <= 0)
            throw new DominioException("La categoría del producto es obligatoria.");

        CategoriaId = categoriaId;
        Sku = NormalizarSku(sku);
        Nombre = Validar.TextoObligatorio(nombre, "Nombre", LongitudMaximaNombre);
        Marca = Validar.TextoOpcional(marca, "Marca", 60);
        Modelo = Validar.TextoOpcional(modelo, "Modelo", 60);
        Descripcion = Validar.TextoOpcional(descripcion, "Descripción", 500);
        EstablecerPrecios(precioCompra, precioVenta);
        EsServicio = esServicio;
        Activo = true;
        FechaCreacionUtc = DateTime.UtcNow;
    }

    public void Actualizar(int categoriaId, string nombre, string? marca, string? modelo, string? descripcion)
    {
        if (categoriaId <= 0)
            throw new DominioException("La categoría del producto es obligatoria.");

        CategoriaId = categoriaId;
        Nombre = Validar.TextoObligatorio(nombre, "Nombre", LongitudMaximaNombre);
        Marca = Validar.TextoOpcional(marca, "Marca", 60);
        Modelo = Validar.TextoOpcional(modelo, "Modelo", 60);
        Descripcion = Validar.TextoOpcional(descripcion, "Descripción", 500);
    }

    public void EstablecerPrecios(decimal precioCompra, decimal precioVenta)
    {
        if (precioCompra < 0)
            throw new DominioException("El precio de compra no puede ser negativo.");

        if (precioVenta <= 0)
            throw new DominioException("El precio de venta debe ser mayor que cero.");

        if (precioVenta < precioCompra)
            throw new DominioException("El precio de venta no puede ser menor que el precio de compra.");

        if (decimal.Round(precioCompra, 2) != precioCompra || decimal.Round(precioVenta, 2) != precioVenta)
            throw new DominioException("Los precios admiten como máximo 2 decimales.");

        PrecioCompra = precioCompra;
        PrecioVenta = precioVenta;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;

    private static string NormalizarSku(string sku)
    {
        var limpio = Validar.TextoObligatorio(sku, "SKU", LongitudMaximaSku).ToUpperInvariant();

        if (!PatronSku.IsMatch(limpio))
            throw new DominioException("El SKU solo puede contener letras, números y guiones.");

        return limpio;
    }
}
