using AMSoluciones.Application.Dtos;
using AMSoluciones.Domain.Repositorios;

namespace AMSoluciones.Application.Servicios;

/// <summary>Consulta de existencias.</summary>
public class InventarioService
{
    private readonly IProductoRepository _productos;
    private readonly IInventarioRepository _inventarios;

    public InventarioService(IProductoRepository productos, IInventarioRepository inventarios)
    {
        _productos = productos;
        _inventarios = inventarios;
    }

    public async Task<IReadOnlyList<ExistenciaDto>> ListarAsync(CancellationToken ct = default)
    {
        var productos = await _productos.ListarAsync(ct);
        var inventarios = (await _inventarios.ListarAsync(ct)).ToDictionary(i => i.ProductoId);

        return productos
            .Where(p => inventarios.ContainsKey(p.Id))
            .Select(p => new ExistenciaDto(p.Id, p.Sku, p.Nombre, inventarios[p.Id].Disponible))
            .ToList();
    }
}
