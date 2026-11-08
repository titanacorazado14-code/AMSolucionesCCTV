using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Excepciones;
using Xunit;

namespace AMSoluciones.Domain.Tests;

public class InventarioTests
{
    [Fact]
    public void Reponer_AumentaElStockDisponible()
    {
        var inventario = new Inventario(productoId: 1, disponible: 5);

        inventario.Reponer(10);

        Assert.Equal(15, inventario.Disponible);
    }

    [Fact]
    public void Reponer_ConCantidadCero_LanzaDominioException()
    {
        var inventario = new Inventario(1, 5);

        Assert.Throws<DominioException>(() => inventario.Reponer(0));
    }

    [Fact]
    public void Descontar_ConStockSuficiente_ReduceElStock()
    {
        var inventario = new Inventario(1, 10);

        inventario.Descontar(4);

        Assert.Equal(6, inventario.Disponible);
    }

    [Fact]
    public void Descontar_MasDeLoDisponible_LanzaStockInsuficienteException()
    {
        var inventario = new Inventario(1, 3);

        var ex = Assert.Throws<StockInsuficienteException>(() => inventario.Descontar(5));

        Assert.Equal(3, ex.Disponible);
        Assert.Equal(5, ex.Solicitado);
        Assert.Equal(3, inventario.Disponible); // el stock no cambió
    }

    [Fact]
    public void EstaBajoMinimo_CuandoDisponibleEsIgualOMenorAlMinimo_EsVerdadero()
    {
        var inventario = new Inventario(1, disponible: 2, stockMinimo: 2);

        Assert.True(inventario.EstaBajoMinimo);
    }
}
