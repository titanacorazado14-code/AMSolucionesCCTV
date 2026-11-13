using AMSoluciones.Application.Dtos;
using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Excepciones;
using AMSoluciones.Domain.Repositorios;

namespace AMSoluciones.Application.CasosDeUso;

/// <summary>
/// Caso de uso: registrar venta.
/// Valida todas las líneas, toma nombre y precio del catálogo (nunca del cliente),
/// descuenta el inventario y guarda venta + detalles + descuentos en una sola transacción.
/// </summary>
public class RegistrarVentaUseCase
{
    private readonly IProductoRepository _productos;
    private readonly IInventarioRepository _inventarios;
    private readonly IVentaRepository _ventas;
    private readonly IUnitOfWork _uow;

    public RegistrarVentaUseCase(
        IProductoRepository productos,
        IInventarioRepository inventarios,
        IVentaRepository ventas,
        IUnitOfWork uow)
    {
        _productos = productos;
        _inventarios = inventarios;
        _ventas = ventas;
        _uow = uow;
    }

    public async Task<VentaDto> EjecutarAsync(VentaInput input, CancellationToken ct = default)
    {
        // 1. Validar la forma de la solicitud.
        if (input.Detalles is null || input.Detalles.Count == 0)
            throw new DominioException("La venta debe tener al menos una línea.", "VENTA_VACIA");

        if (input.Detalles.Count > Venta.MaximoLineas)
            throw new DominioException($"Una venta admite como máximo {Venta.MaximoLineas} líneas.", "DEMASIADAS_LINEAS");

        var ids = input.Detalles.Select(l => l.ProductoId).ToList();
        if (ids.Distinct().Count() != ids.Count)
            throw new DominioException("La venta no admite productos repetidos.", "LINEA_REPETIDA");

        // 2. Cargar productos e inventarios; todos deben existir.
        var productos = (await _productos.ObtenerPorIdsAsync(ids, ct)).ToDictionary(p => p.Id);
        var inventarios = (await _inventarios.ObtenerPorProductoIdsAsync(ids, ct)).ToDictionary(i => i.ProductoId);

        foreach (var id in ids)
        {
            if (!productos.ContainsKey(id))
                throw new NoEncontradoException("el producto", id);
            if (!inventarios.ContainsKey(id))
                throw new NoEncontradoException("el inventario del producto", id);
        }

        // 3. Verificar existencias de TODAS las líneas antes de cambiar nada.
        foreach (var linea in input.Detalles)
            inventarios[linea.ProductoId].VerificarDisponibilidad(linea.Cantidad);

        // 4. Armar la venta con los precios vigentes del servidor.
        var venta = new Venta();
        foreach (var linea in input.Detalles)
        {
            var producto = productos[linea.ProductoId];
            venta.AgregarDetalle(producto.Id, producto.Nombre, linea.Cantidad, producto.Precio);
        }
        venta.Confirmar();

        // 5. Descontar el inventario.
        foreach (var linea in input.Detalles)
            inventarios[linea.ProductoId].Descontar(linea.Cantidad);

        // 6. Guardar todo en una transacción. Si otra venta consumió las mismas unidades,
        //    el rowversion detecta el choque, se revierte todo y se lanza ConcurrenciaException (409).
        await _ventas.AgregarAsync(venta, ct);
        await _uow.GuardarCambiosAsync(ct);

        return venta.ADto();
    }
}
