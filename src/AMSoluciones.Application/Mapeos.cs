using AMSoluciones.Application.Dtos;
using AMSoluciones.Domain.Entidades;

namespace AMSoluciones.Application;

/// <summary>Convierte entidades de Domain en DTOs. Domain nunca sale directo hacia la API.</summary>
public static class Mapeos
{
    public static CategoriaDto ADto(this Categoria c) => new(c.Id, c.Nombre);

    public static ProductoDto ADto(this Producto p) => new(p.Id, p.CategoriaId, p.Sku, p.Nombre, p.Precio);

    public static DetalleVentaDto ADto(this DetalleVenta d) =>
        new(d.ProductoId, d.ProductoNombre, d.Cantidad, d.PrecioUnitario, d.Subtotal);

    public static VentaDto ADto(this Venta v) =>
        new(v.Id, v.FechaUtc, v.Estado.ToString(), v.Total, v.AnuladaUtc, v.Detalles.Select(d => d.ADto()).ToList());
}
