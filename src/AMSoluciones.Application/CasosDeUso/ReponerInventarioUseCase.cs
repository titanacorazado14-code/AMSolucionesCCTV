using AMSoluciones.Application.Dtos;
using AMSoluciones.Domain.Excepciones;
using AMSoluciones.Domain.Repositorios;

namespace AMSoluciones.Application.CasosDeUso;

/// <summary>Caso de uso: reponer inventario.</summary>
public class ReponerInventarioUseCase
{
    private readonly IProductoRepository _productos;
    private readonly IInventarioRepository _inventarios;
    private readonly IUnitOfWork _uow;

    public ReponerInventarioUseCase(IProductoRepository productos, IInventarioRepository inventarios, IUnitOfWork uow)
    {
        _productos = productos;
        _inventarios = inventarios;
        _uow = uow;
    }

    public async Task<ExistenciaDto> EjecutarAsync(int productoId, ReposicionInput input, CancellationToken ct = default)
    {
        var producto = await _productos.ObtenerPorIdAsync(productoId, ct)
            ?? throw new NoEncontradoException("el producto", productoId);

        var inventario = await _inventarios.ObtenerPorProductoIdAsync(productoId, ct)
            ?? throw new NoEncontradoException("el inventario del producto", productoId);

        inventario.Reponer(input.Cantidad);   // la regla (1 a 1,000,000 y tope de existencias) vive en Domain

        await _uow.GuardarCambiosAsync(ct);

        return new ExistenciaDto(producto.Id, producto.Sku, producto.Nombre, inventario.Disponible);
    }
}
