using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Excepciones;
using Xunit;

namespace AMSoluciones.Domain.Tests;

public class InventarioTests
{
    [Fact]
    public void Reponer_AumentaLasExistencias()
    {
        var inventario = new Inventario(productoId: 1, disponible: 18);

        inventario.Reponer(5);

        Assert.Equal(23, inventario.Disponible);
    }

    [Fact]
    public void Reponer_ConCantidadCero_LanzaDominioException()
    {
        var inventario = new Inventario(1, 18);

        Assert.Throws<DominioException>(() => inventario.Reponer(0));
        Assert.Equal(18, inventario.Disponible);
    }

    [Fact]
    public void Reponer_QueExcedeElLimite_LanzaLimiteInventarioException()
    {
        var inventario = new Inventario(1, Inventario.Maximo - 1);

        Assert.Throws<LimiteInventarioException>(() => inventario.Reponer(2));
        Assert.Equal(Inventario.Maximo - 1, inventario.Disponible);
    }

    [Fact]
    public void Descontar_MasDeLoDisponible_LanzaStockInsuficienteException()
    {
        var inventario = new Inventario(1, 3);

        var ex = Assert.Throws<StockInsuficienteException>(() => inventario.Descontar(5));

        Assert.Equal(3, ex.Disponible);
        Assert.Equal(5, ex.Solicitado);
        Assert.Equal(3, inventario.Disponible);   // el stock no cambió
    }
}
