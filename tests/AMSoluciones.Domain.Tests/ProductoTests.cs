using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Excepciones;
using Xunit;

namespace AMSoluciones.Domain.Tests;

public class ProductoTests
{
    [Fact]
    public void Crear_ConDatosValidos_NormalizaElSkuAMayusculas()
    {
        var producto = new Producto(1, "  cam-001 ", "Cámara bala 2MP", 1450m);

        Assert.Equal("CAM-001", producto.Sku);
        Assert.Equal(1450m, producto.Precio);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1_000_000)]
    public void Crear_ConPrecioFueraDeRango_LanzaDominioException(int precio)
    {
        Assert.Throws<DominioException>(() => new Producto(1, "CAM-001", "Cámara bala 2MP", precio));
    }

    [Fact]
    public void Crear_ConMasDeDosDecimales_LanzaDominioException()
    {
        Assert.Throws<DominioException>(() => new Producto(1, "CAM-001", "Cámara bala 2MP", 10.123m));
    }

    [Theory]
    [InlineData("PAP 001")]
    [InlineData("PAP_001")]
    [InlineData("PAÑ-001")]
    [InlineData("   ")]
    public void Crear_ConSkuInvalido_LanzaDominioException(string sku)
    {
        Assert.Throws<DominioException>(() => new Producto(1, sku, "Cámara bala 2MP", 1450m));
    }
}
