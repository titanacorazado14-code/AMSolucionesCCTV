using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Excepciones;
using Xunit;

namespace AMSoluciones.Domain.Tests;

public class ProductoTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaElSkuAMayusculas()
    {
        var producto = new Producto(1, "  cam-bala-2mp ", "Cámara bala 2MP", 950m, 1450m);

        Assert.Equal("CAM-BALA-2MP", producto.Sku);
        Assert.True(producto.Activo);
    }

    [Fact]
    public void Crear_ConPrecioVentaMenorAlCosto_LanzaDominioException()
    {
        Assert.Throws<DominioException>(() =>
            new Producto(1, "CAM-01", "Cámara", precioCompra: 1000m, precioVenta: 900m));
    }

    [Fact]
    public void Crear_ConPrecioVentaCero_LanzaDominioException()
    {
        Assert.Throws<DominioException>(() =>
            new Producto(1, "CAM-01", "Cámara", precioCompra: 0m, precioVenta: 0m));
    }

    [Theory]
    [InlineData("CAM 01")]
    [InlineData("CAM_01")]
    [InlineData("   ")]
    public void Crear_ConSkuInvalido_LanzaDominioException(string sku)
    {
        Assert.Throws<DominioException>(() =>
            new Producto(1, sku, "Cámara", 100m, 200m));
    }
}
