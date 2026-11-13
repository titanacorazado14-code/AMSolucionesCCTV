using System.Text.RegularExpressions;
using AMSoluciones.Domain.Comun;
using AMSoluciones.Domain.Excepciones;

namespace AMSoluciones.Domain.Entidades;

public class Producto
{
    public const int LongitudMaximaSku = 32;
    public const int LongitudMaximaNombre = 120;
    public const decimal PrecioMaximo = 999_999.99m;

    // Solo letras ASCII mayúsculas, dígitos y guiones.
    private static readonly Regex PatronSku = new(@"^[A-Z0-9-]+\z", RegexOptions.Compiled);

    public int Id { get; private set; }
    public int CategoriaId { get; private set; }
    public Categoria? Categoria { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public decimal Precio { get; private set; }

    // Constructor vacío solo para Entity Framework.
    private Producto()
    {
    }

    public Producto(int categoriaId, string sku, string nombre, decimal precio)
    {
        Aplicar(categoriaId, sku, nombre, precio);
    }

    public void Actualizar(int categoriaId, string sku, string nombre, decimal precio)
    {
        Aplicar(categoriaId, sku, nombre, precio);
    }

    /// <summary>
    /// Recorta el SKU, lo pasa a mayúsculas y verifica que solo tenga letras, números y guiones.
    /// </summary>
    public static string NormalizarSku(string sku)
    {
        var limpio = Validar.TextoObligatorio(sku, "SKU", LongitudMaximaSku).ToUpperInvariant();

        if (!PatronSku.IsMatch(limpio))
            throw new DominioException("El SKU solo puede contener letras (A-Z), números y guiones.", "SKU_INVALIDO");

        return limpio;
    }

    private void Aplicar(int categoriaId, string sku, string nombre, decimal precio)
    {
        if (categoriaId <= 0)
            throw new DominioException("La categoría del producto es obligatoria.", "CATEGORIA_OBLIGATORIA");

        if (precio <= 0 || precio > PrecioMaximo)
            throw new DominioException($"El precio debe ser mayor que cero y no superar RD${PrecioMaximo:N2}.", "PRECIO_INVALIDO");

        if (decimal.Round(precio, 2) != precio)
            throw new DominioException("El precio admite como máximo dos decimales.", "PRECIO_INVALIDO");

        CategoriaId = categoriaId;
        Sku = NormalizarSku(sku);
        Nombre = Validar.TextoObligatorio(nombre, "Nombre", LongitudMaximaNombre);
        Precio = precio;
    }
}
