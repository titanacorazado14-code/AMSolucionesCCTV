using AMSoluciones.Application.Dtos;
using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Excepciones;
using AMSoluciones.Domain.Repositorios;

namespace AMSoluciones.Application.Servicios;

/// <summary>CRUD de productos. Un producto nuevo nace con su inventario en cero.</summary>
public class ProductoService
{
    private readonly IProductoRepository _productos;
    private readonly ICategoriaRepository _categorias;
    private readonly IInventarioRepository _inventarios;
    private readonly IUnitOfWork _uow;

    public ProductoService(
        IProductoRepository productos,
        ICategoriaRepository categorias,
        IInventarioRepository inventarios,
        IUnitOfWork uow)
    {
        _productos = productos;
        _categorias = categorias;
        _inventarios = inventarios;
        _uow = uow;
    }

    public async Task<IReadOnlyList<ProductoDto>> ListarAsync(CancellationToken ct = default)
    {
        var productos = await _productos.ListarAsync(ct);
        return productos.Select(p => p.ADto()).ToList();
    }

    public async Task<ProductoDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var producto = await _productos.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("el producto", id);
        return producto.ADto();
    }

    public async Task<ProductoDto> CrearAsync(ProductoInput input, CancellationToken ct = default)
    {
        await AsegurarCategoriaAsync(input.CategoriaId, ct);

        var producto = new Producto(input.CategoriaId, input.Sku, input.Nombre, input.Precio);   // valida todo

        if (await _productos.ExistePorSkuAsync(producto.Sku, null, ct))
            throw new ConflictoException($"Ya existe un producto con el SKU '{producto.Sku}'.", "SKU_DUPLICADO");

        // Producto e inventario inicial en cero se crean en la misma transacción.
        await _uow.EjecutarEnTransaccionAsync(async () =>
        {
            await _productos.AgregarAsync(producto, ct);
            await _uow.GuardarCambiosAsync(ct);   // aquí se genera el Id del producto

            await _inventarios.AgregarAsync(new Inventario(producto.Id), ct);
            await _uow.GuardarCambiosAsync(ct);
        }, ct);

        return producto.ADto();
    }

    public async Task<ProductoDto> ActualizarAsync(int id, ProductoInput input, CancellationToken ct = default)
    {
        var producto = await _productos.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("el producto", id);

        await AsegurarCategoriaAsync(input.CategoriaId, ct);

        var skuNormalizado = Producto.NormalizarSku(input.Sku);
        if (await _productos.ExistePorSkuAsync(skuNormalizado, id, ct))
            throw new ConflictoException($"Ya existe un producto con el SKU '{skuNormalizado}'.", "SKU_DUPLICADO");

        producto.Actualizar(input.CategoriaId, input.Sku, input.Nombre, input.Precio);
        await _uow.GuardarCambiosAsync(ct);
        return producto.ADto();
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var producto = await _productos.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("el producto", id);

        var inventario = await _inventarios.ObtenerPorProductoIdAsync(id, ct);
        if (inventario is not null && inventario.Disponible != 0)
            throw new ConflictoException("Solo se puede eliminar un producto con el inventario en cero.", "PRODUCTO_CON_EXISTENCIAS");

        if (await _productos.TieneDetallesAsync(id, ct))
            throw new ConflictoException("No se puede eliminar un producto que aparece en ventas, aunque estén anuladas.", "PRODUCTO_CON_HISTORIAL");

        // Inventario y producto se eliminan en el mismo guardado (una sola transacción).
        if (inventario is not null)
            _inventarios.Eliminar(inventario);

        _productos.Eliminar(producto);
        await _uow.GuardarCambiosAsync(ct);
    }

    private async Task AsegurarCategoriaAsync(int categoriaId, CancellationToken ct)
    {
        if (await _categorias.ObtenerPorIdAsync(categoriaId, ct) is null)
            throw new NoEncontradoException("la categoría", categoriaId);
    }
}
