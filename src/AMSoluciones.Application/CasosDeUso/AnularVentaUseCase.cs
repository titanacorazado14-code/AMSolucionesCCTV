using AMSoluciones.Application.Dtos;
using AMSoluciones.Domain.Enums;
using AMSoluciones.Domain.Excepciones;
using AMSoluciones.Domain.Repositorios;

namespace AMSoluciones.Application.CasosDeUso;

/// <summary>
/// Caso de uso: anular venta.
/// Marca la venta como anulada y devuelve al inventario las unidades de todas sus líneas,
/// una sola vez y en una sola transacción. Conserva el total y los detalles históricos.
/// </summary>
public class AnularVentaUseCase
{
    private readonly IVentaRepository _ventas;
    private readonly IInventarioRepository _inventarios;
    private readonly IUnitOfWork _uow;

    public AnularVentaUseCase(IVentaRepository ventas, IInventarioRepository inventarios, IUnitOfWork uow)
    {
        _ventas = ventas;
        _inventarios = inventarios;
        _uow = uow;
    }

    public async Task<VentaDto> EjecutarAsync(int ventaId, CancellationToken ct = default)
    {
        var venta = await _ventas.ObtenerPorIdAsync(ventaId, ct)
            ?? throw new NoEncontradoException("la venta", ventaId);

        if (venta.Estado == EstadoVenta.Anulada)
            throw new VentaYaAnuladaException(venta.Id);

        var ids = venta.Detalles.Select(d => d.ProductoId).ToList();
        var inventarios = (await _inventarios.ObtenerPorProductoIdsAsync(ids, ct)).ToDictionary(i => i.ProductoId);

        // Primero se verifica que todas las devoluciones quepan; recién después se cambia algo.
        foreach (var detalle in venta.Detalles)
        {
            if (!inventarios.TryGetValue(detalle.ProductoId, out var inventario))
                throw new NoEncontradoException("el inventario del producto", detalle.ProductoId);

            inventario.VerificarCapacidad(detalle.Cantidad);
        }

        venta.Anular();

        foreach (var detalle in venta.Detalles)
            inventarios[detalle.ProductoId].Reponer(detalle.Cantidad);

        await _uow.GuardarCambiosAsync(ct);

        return venta.ADto();
    }
}
