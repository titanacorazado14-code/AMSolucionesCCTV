using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Enums;
using AMSoluciones.Domain.Excepciones;
using Xunit;

namespace AMSoluciones.Domain.Tests;

public class VentaTests
{
    [Fact]
    public void AgregarDetalle_CalculaSubtotalYTotal()
    {
        var venta = new Venta();

        venta.AgregarDetalle(1, "Cámara bala 2MP", 2, 1450m);
        venta.AgregarDetalle(2, "Fuente 12V 2A", 2, 320m);

        Assert.Equal(2, venta.Detalles.Count);
        Assert.Equal(2900m + 640m, venta.Total);
    }

    [Fact]
    public void AgregarDetalle_ConProductoRepetido_LanzaDominioException()
    {
        var venta = new Venta();
        venta.AgregarDetalle(1, "Cámara bala 2MP", 1, 1450m);

        Assert.Throws<DominioException>(() =>
            venta.AgregarDetalle(1, "Cámara bala 2MP", 1, 1450m));
    }

    [Fact]
    public void AgregarDetalle_ConCantidadCero_LanzaDominioException()
    {
        var venta = new Venta();

        Assert.Throws<DominioException>(() =>
            venta.AgregarDetalle(1, "Cámara bala 2MP", 0, 1450m));
    }

    [Fact]
    public void Confirmar_SinDetalles_LanzaDominioException()
    {
        var venta = new Venta();

        Assert.Throws<DominioException>(() => venta.Confirmar());
    }

    [Fact]
    public void Anular_CambiaElEstadoYRegistraLaFecha()
    {
        var venta = new Venta();
        venta.AgregarDetalle(1, "Cámara bala 2MP", 1, 1450m);

        venta.Anular();

        Assert.Equal(EstadoVenta.Anulada, venta.Estado);
        Assert.NotNull(venta.AnuladaUtc);
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
