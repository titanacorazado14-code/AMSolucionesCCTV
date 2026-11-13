using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Enums;
using AMSoluciones.Domain.Excepciones;
using Xunit;

namespace AMSoluciones.Domain.Tests;

public class VentaTests
{
    [Fact]
    public void AgregarDetalle_CalculaElTotalDeVariasLineas()
    {
        var venta = new Venta();

        venta.AgregarDetalle(1, "Cámara bala 2MP", 2, 1450m);   // 2900
        venta.AgregarDetalle(2, "Conector BNC", 4, 25m);   // 100

        Assert.Equal(2, venta.Detalles.Count);
        Assert.Equal(3000m, venta.Total);
    }

    [Fact]
    public void AgregarDetalle_ConProductoRepetido_LanzaDominioException()
    {
        var venta = new Venta();
        venta.AgregarDetalle(1, "Cámara bala 2MP", 1, 1450m);

        Assert.Throws<DominioException>(() => venta.AgregarDetalle(1, "Cámara bala 2MP", 1, 1450m));
    }

    [Fact]
    public void AgregarDetalle_ConCantidadCero_LanzaDominioException()
    {
        var venta = new Venta();

        Assert.Throws<DominioException>(() => venta.AgregarDetalle(1, "Cámara bala 2MP", 0, 1450m));
    }

    [Fact]
    public void AgregarDetalle_ConMasDeCincuentaLineas_LanzaDominioException()
    {
        var venta = new Venta();
        for (var i = 1; i <= Venta.MaximoLineas; i++)
            venta.AgregarDetalle(i, $"Producto {i}", 1, 10m);

        Assert.Throws<DominioException>(() => venta.AgregarDetalle(999, "Otro", 1, 10m));
    }

    [Fact]
    public void Confirmar_SinLineas_LanzaDominioException()
    {
        var venta = new Venta();

        Assert.Throws<DominioException>(() => venta.Confirmar());
    }

    [Fact]
    public void Anular_ConservaElTotalYLosDetalles()
    {
        var venta = new Venta();
        venta.AgregarDetalle(1, "Cámara bala 2MP", 2, 1450m);

        venta.Anular();

        Assert.Equal(EstadoVenta.Anulada, venta.Estado);
        Assert.NotNull(venta.AnuladaUtc);
        Assert.Equal(2900m, venta.Total);
        Assert.Single(venta.Detalles);
    }

    [Fact]
    public void Anular_UnaVentaYaAnulada_LanzaVentaYaAnuladaException()
    {
        var venta = new Venta();
        venta.AgregarDetalle(1, "Cámara bala 2MP", 1, 1450m);
        venta.Anular();

        Assert.Throws<VentaYaAnuladaException>(() => venta.Anular());
    }
}
