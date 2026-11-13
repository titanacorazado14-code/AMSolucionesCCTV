using System.Reflection;
using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Repositorios;

namespace AMSoluciones.Application.Tests;

// Repositorios falsos en memoria: permiten probar los casos de uso sin base de datos.

internal static class Ids
{
    private static int _siguiente = 1;

    /// <summary>Las entidades tienen setters privados; la base de datos asigna el Id. Aquí lo simulamos.</summary>
    public static void Asignar(object entidad)
    {
        var propiedad = entidad.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!;
        propiedad.SetValue(entidad, _siguiente++);
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int Guardados { get; private set; }

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default)
    {
        Guardados++;
        return Task.FromResult(1);
    }

    public Task EjecutarEnTransaccionAsync(Func<Task> operacion, CancellationToken ct = default) => operacion();
}

internal sealed class FakeCategoriaRepository : ICategoriaRepository
{
    public List<Categoria> Lista { get; } = new();

    public Task<Categoria?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Lista.FirstOrDefault(c => c.Id == id));

    public Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Categoria>>(Lista.ToList());

    public Task<bool> ExistePorNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default) =>
        Task.FromResult(Lista.Any(c => string.Equals(c.Nombre, nombre.Trim(), StringComparison.OrdinalIgnoreCase)
                                       && (excluirId == null || c.Id != excluirId)));

    public Task<bool> TieneProductosAsync(int categoriaId, CancellationToken ct = default) => Task.FromResult(false);

    public Task AgregarAsync(Categoria categoria, CancellationToken ct = default)
    {
        Ids.Asignar(categoria);
        Lista.Add(categoria);
        return Task.CompletedTask;
    }

    public void Eliminar(Categoria categoria) => Lista.Remove(categoria);
}

internal sealed class FakeProductoRepository : IProductoRepository
{
    public List<Producto> Lista { get; } = new();

    public Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Lista.FirstOrDefault(p => p.Id == id));

    public Task<IReadOnlyList<Producto>> ObtenerPorIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var conjunto = ids.ToHashSet();
        return Task.FromResult<IReadOnlyList<Producto>>(Lista.Where(p => conjunto.Contains(p.Id)).ToList());
    }

    public Task<IReadOnlyList<Producto>> ListarAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Producto>>(Lista.ToList());

    public Task<bool> ExistePorSkuAsync(string sku, int? excluirId = null, CancellationToken ct = default) =>
        Task.FromResult(Lista.Any(p => string.Equals(p.Sku, sku.Trim(), StringComparison.OrdinalIgnoreCase)
                                       && (excluirId == null || p.Id != excluirId)));

    public Task<bool> TieneDetallesAsync(int productoId, CancellationToken ct = default) => Task.FromResult(false);

    public Task AgregarAsync(Producto producto, CancellationToken ct = default)
    {
        Ids.Asignar(producto);
        Lista.Add(producto);
        return Task.CompletedTask;
    }

    public void Eliminar(Producto producto) => Lista.Remove(producto);
}

internal sealed class FakeInventarioRepository : IInventarioRepository
{
    public List<Inventario> Lista { get; } = new();

    public Task<Inventario?> ObtenerPorProductoIdAsync(int productoId, CancellationToken ct = default) =>
        Task.FromResult(Lista.FirstOrDefault(i => i.ProductoId == productoId));

    public Task<IReadOnlyList<Inventario>> ObtenerPorProductoIdsAsync(IEnumerable<int> productoIds, CancellationToken ct = default)
    {
        var conjunto = productoIds.ToHashSet();
        return Task.FromResult<IReadOnlyList<Inventario>>(Lista.Where(i => conjunto.Contains(i.ProductoId)).ToList());
    }

    public Task<IReadOnlyList<Inventario>> ListarAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Inventario>>(Lista.ToList());

    public Task AgregarAsync(Inventario inventario, CancellationToken ct = default)
    {
        Lista.Add(inventario);
        return Task.CompletedTask;
    }

    public void Eliminar(Inventario inventario) => Lista.Remove(inventario);
}

internal sealed class FakeVentaRepository : IVentaRepository
{
    public List<Venta> Lista { get; } = new();

    public Task<Venta?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Lista.FirstOrDefault(v => v.Id == id));

    public Task<IReadOnlyList<Venta>> ListarAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Venta>>(Lista.ToList());

    public Task AgregarAsync(Venta venta, CancellationToken ct = default)
    {
        Ids.Asignar(venta);
        Lista.Add(venta);
        return Task.CompletedTask;
    }
}

/// <summary>Escenario de prueba: Cámara bala 2MP (18 en stock, RD$1,450) y Conector BNC (47 en stock, RD$25).</summary>
internal sealed class Escenario
{
    public FakeUnitOfWork Uow { get; } = new();
    public FakeCategoriaRepository Categorias { get; } = new();
    public FakeProductoRepository Productos { get; } = new();
    public FakeInventarioRepository Inventarios { get; } = new();
    public FakeVentaRepository Ventas { get; } = new();

    public int CategoriaId { get; }
    public int CamaraId { get; }
    public int ConectorId { get; }

    public Escenario()
    {
        var categoria = new Categoria("Cámaras");
        Ids.Asignar(categoria);
        Categorias.Lista.Add(categoria);
        CategoriaId = categoria.Id;

        var camara = new Producto(categoria.Id, "CAM-001", "Cámara bala 2MP", 1450m);
        Ids.Asignar(camara);
        Productos.Lista.Add(camara);
        CamaraId = camara.Id;
        Inventarios.Lista.Add(new Inventario(camara.Id, 18));

        var conector = new Producto(categoria.Id, "CON-001", "Conector BNC", 25m);
        Ids.Asignar(conector);
        Productos.Lista.Add(conector);
        ConectorId = conector.Id;
        Inventarios.Lista.Add(new Inventario(conector.Id, 47));
    }

    public int Stock(int productoId) => Inventarios.Lista.Single(i => i.ProductoId == productoId).Disponible;
}
