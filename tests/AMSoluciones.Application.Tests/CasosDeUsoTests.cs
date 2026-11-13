using AMSoluciones.Application.CasosDeUso;
using AMSoluciones.Application.Dtos;
using AMSoluciones.Application.Servicios;
using AMSoluciones.Domain.Excepciones;
using Xunit;

namespace AMSoluciones.Application.Tests;

public class CasosDeUsoTests
{
    [Fact]
    public async Task CrearProducto_DejaSuInventarioEnCero()
    {
        var e = new Escenario();
        var servicio = new ProductoService(e.Productos, e.Categorias, e.Inventarios, e.Uow);

        var creado = await servicio.CrearAsync(new ProductoInput(e.CategoriaId, "tec-003", "Mouse", 300m));

        Assert.Equal("TEC-003", creado.Sku);
        Assert.Equal(0, e.Stock(creado.Id));
    }

    [Fact]
    public async Task ReponerInventario_AumentaSoloElProductoIndicado()
    {
        var e = new Escenario();
        var reponer = new ReponerInventarioUseCase(e.Productos, e.Inventarios, e.Uow);

        var resultado = await reponer.EjecutarAsync(e.CamaraId, new ReposicionInput(5));

        Assert.Equal(23, resultado.Disponible);
        Assert.Equal(23, e.Stock(e.CamaraId));
        Assert.Equal(47, e.Stock(e.ConectorId));   // el otro producto no cambió

        var existencias = await new InventarioService(e.Productos, e.Inventarios).ListarAsync();
        Assert.Equal(23, existencias.Single(x => x.ProductoId == e.CamaraId).Disponible);
    }

    [Fact]
    public async Task RegistrarVenta_UsaLosPreciosDelServidorYDescuentaElInventario()
    {
        var e = new Escenario();
        var registrar = new RegistrarVentaUseCase(e.Productos, e.Inventarios, e.Ventas, e.Uow);

        var venta = await registrar.EjecutarAsync(new VentaInput(new List<LineaVentaInput>
        {
            new(e.CamaraId, 2),
            new(e.ConectorId, 4)
        }));

        Assert.Equal(3000m, venta.Total);
        Assert.Equal("Confirmada", venta.Estado);
        Assert.Equal(1450m, venta.Detalles.Single(d => d.ProductoId == e.CamaraId).PrecioUnitario);
        Assert.Equal(16, e.Stock(e.CamaraId));
        Assert.Equal(43, e.Stock(e.ConectorId));
    }

    [Fact]
    public async Task RegistrarVenta_ConUnaLineaSinStock_SeRechazaYConservaTodoElInventario()
    {
        var e = new Escenario();
        var registrar = new RegistrarVentaUseCase(e.Productos, e.Inventarios, e.Ventas, e.Uow);

        await Assert.ThrowsAsync<StockInsuficienteException>(() =>
            registrar.EjecutarAsync(new VentaInput(new List<LineaVentaInput>
            {
                new(e.CamaraId, 2),
                new(e.ConectorId, 48)   // solo hay 47
            })));

        Assert.Equal(18, e.Stock(e.CamaraId));    // ni siquiera la línea válida se descontó
        Assert.Equal(47, e.Stock(e.ConectorId));
        Assert.Empty(e.Ventas.Lista);
        Assert.Equal(0, e.Uow.Guardados);
    }

    [Fact]
    public async Task AnularVenta_DevuelveLasCantidadesUnaSolaVez()
    {
        var e = new Escenario();
        var registrar = new RegistrarVentaUseCase(e.Productos, e.Inventarios, e.Ventas, e.Uow);
        var anular = new AnularVentaUseCase(e.Ventas, e.Inventarios, e.Uow);

        var venta = await registrar.EjecutarAsync(new VentaInput(new List<LineaVentaInput>
        {
            new(e.CamaraId, 2),
            new(e.ConectorId, 4)
        }));

        var anulada = await anular.EjecutarAsync(venta.Id);

        Assert.Equal("Anulada", anulada.Estado);
        Assert.Equal(3000m, anulada.Total);          // el total histórico se conserva
        Assert.Equal(2, anulada.Detalles.Count);    // los detalles también
        Assert.Equal(18, e.Stock(e.CamaraId));
        Assert.Equal(47, e.Stock(e.ConectorId));

        // Una segunda anulación se rechaza y no devuelve unidades otra vez.
        await Assert.ThrowsAsync<VentaYaAnuladaException>(() => anular.EjecutarAsync(venta.Id));
        Assert.Equal(18, e.Stock(e.CamaraId));
    }
}
