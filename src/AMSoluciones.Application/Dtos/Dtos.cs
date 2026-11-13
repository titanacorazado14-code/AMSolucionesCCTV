namespace AMSoluciones.Application.Dtos;

// Los nombres y campos siguen el contrato de la API de la asignatura (CONTRATO_API.md).

public sealed record CategoriaDto(int Id, string Nombre);
public sealed record CategoriaInput(string Nombre);

public sealed record ProductoDto(int Id, int CategoriaId, string Sku, string Nombre, decimal Precio);
public sealed record ProductoInput(int CategoriaId, string Sku, string Nombre, decimal Precio);

public sealed record ExistenciaDto(int ProductoId, string Sku, string ProductoNombre, int Disponible);
public sealed record ReposicionInput(int Cantidad);

public sealed record LineaVentaInput(int ProductoId, int Cantidad);
public sealed record VentaInput(List<LineaVentaInput> Detalles);

public sealed record DetalleVentaDto(int ProductoId, string ProductoNombre, int Cantidad, decimal PrecioUnitario, decimal Subtotal);
public sealed record VentaDto(
    int Id,
    DateTimeOffset FechaUtc,
    string Estado,
    decimal Total,
    DateTimeOffset? AnuladaUtc,
    IReadOnlyList<DetalleVentaDto> Detalles);

public sealed record ResumenDto(int Productos, long Unidades, int VentasConfirmadas, decimal TotalConfirmado);

public sealed record LoginInput(string Email, string Contrasena);
public sealed record SesionDto(string AccessToken, string Nombre, string[] Roles);
