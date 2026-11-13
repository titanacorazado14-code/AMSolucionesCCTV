using AMSoluciones.Application.Dtos;
using AMSoluciones.Domain.Excepciones;
using AMSoluciones.Domain.Repositorios;

namespace AMSoluciones.Application.Servicios;

/// <summary>Consulta de ventas (con su detalle histórico).</summary>
public class VentaService
{
    private readonly IVentaRepository _ventas;

    public VentaService(IVentaRepository ventas)
    {
        _ventas = ventas;
    }

    public async Task<IReadOnlyList<VentaDto>> ListarAsync(CancellationToken ct = default)
    {
        var ventas = await _ventas.ListarAsync(ct);
        return ventas.Select(v => v.ADto()).ToList();
    }

    public async Task<VentaDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var venta = await _ventas.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("la venta", id);
        return venta.ADto();
    }
}
