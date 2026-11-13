using AMSoluciones.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AMSoluciones.Infrastructure.Tests;

/// <summary>Pruebas contra SQL Server real: transacciones y concurrencia.</summary>
public class PersistenciaSqlServerTests
{
    private static async Task<int> CrearProductoConStockAsync(BaseDatosPrueba bd, int stock)
    {
        await using var db = bd.Crear();

        var categoria = new Categoria("Cámaras");
        db.Categorias.Add(categoria);
        await db.SaveChangesAsync();

        var producto = new Producto(categoria.Id, "CAM-001", "Cámara bala 2MP", 1450m);
        db.Productos.Add(producto);
        await db.SaveChangesAsync();

        db.Inventarios.Add(new Inventario(producto.Id, stock));
        await db.SaveChangesAsync();

        return producto.Id;
    }

    [Fact]
    public async Task ErrorDePersistenciaEnUnaVenta_RevierteTodoElGuardado()
    {
        await using var bd = await BaseDatosPrueba.CrearAsync();
        var productoId = await CrearProductoConStockAsync(bd, stock: 10);

        await using (var db = bd.Crear())
        {
            var inventario = await db.Inventarios.SingleAsync(i => i.ProductoId == productoId);
            inventario.Descontar(3);

            var venta = new Venta();
            venta.AgregarDetalle(productoId, "Cámara bala 2MP", 3, 1450m);
            venta.AgregarDetalle(999_999, "Producto inexistente", 1, 10m);   // viola la clave foránea
            db.Ventas.Add(venta);

            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }

        // Rollback completo: el inventario conserva sus 10 unidades y no quedó ninguna venta.
        await using (var db = bd.Crear())
        {
            Assert.Equal(10, (await db.Inventarios.SingleAsync()).Disponible);
            Assert.Equal(0, await db.Ventas.CountAsync());
            Assert.Equal(0, await db.DetallesVenta.CountAsync());
        }
    }

    [Fact]
    public async Task DosVentasSobreElMismoStock_NoProducenSobreventa()
    {
        await using var bd = await BaseDatosPrueba.CrearAsync();
        var productoId = await CrearProductoConStockAsync(bd, stock: 5);

        await using var dbA = bd.Crear();
        await using var dbB = bd.Crear();

        // Ambas "solicitudes" leen el mismo inventario (5 unidades) y quieren venderlas todas.
        var inventarioA = await dbA.Inventarios.SingleAsync(i => i.ProductoId == productoId);
        var inventarioB = await dbB.Inventarios.SingleAsync(i => i.ProductoId == productoId);

        inventarioA.Descontar(5);
        inventarioB.Descontar(5);

        await dbA.SaveChangesAsync();   // la primera gana

        // La segunda falla: el rowversion cambió desde que la leyó.
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => dbB.SaveChangesAsync());

        await using var verificacion = bd.Crear();
        var final = await verificacion.Inventarios.SingleAsync();
        Assert.Equal(0, final.Disponible);   // sin saldo negativo ni sobreventa
    }
}
