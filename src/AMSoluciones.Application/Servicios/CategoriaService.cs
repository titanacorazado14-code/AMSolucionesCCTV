using AMSoluciones.Application.Dtos;
using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Excepciones;
using AMSoluciones.Domain.Repositorios;

namespace AMSoluciones.Application.Servicios;

/// <summary>CRUD de categorías.</summary>
public class CategoriaService
{
    private readonly ICategoriaRepository _categorias;
    private readonly IUnitOfWork _uow;

    public CategoriaService(ICategoriaRepository categorias, IUnitOfWork uow)
    {
        _categorias = categorias;
        _uow = uow;
    }

    public async Task<IReadOnlyList<CategoriaDto>> ListarAsync(CancellationToken ct = default)
    {
        var categorias = await _categorias.ListarAsync(ct);
        return categorias.Select(c => c.ADto()).ToList();
    }

    public async Task<CategoriaDto> ObtenerAsync(int id, CancellationToken ct = default)
    {
        var categoria = await _categorias.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("la categoría", id);
        return categoria.ADto();
    }

    public async Task<CategoriaDto> CrearAsync(CategoriaInput input, CancellationToken ct = default)
    {
        var categoria = new Categoria(input.Nombre);   // valida el nombre

        if (await _categorias.ExistePorNombreAsync(categoria.Nombre, null, ct))
            throw new ConflictoException($"Ya existe una categoría llamada '{categoria.Nombre}'.", "CATEGORIA_DUPLICADA");

        await _categorias.AgregarAsync(categoria, ct);
        await _uow.GuardarCambiosAsync(ct);
        return categoria.ADto();
    }

    public async Task<CategoriaDto> ActualizarAsync(int id, CategoriaInput input, CancellationToken ct = default)
    {
        var categoria = await _categorias.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("la categoría", id);

        categoria.Renombrar(input.Nombre);

        if (await _categorias.ExistePorNombreAsync(categoria.Nombre, id, ct))
            throw new ConflictoException($"Ya existe una categoría llamada '{categoria.Nombre}'.", "CATEGORIA_DUPLICADA");

        await _uow.GuardarCambiosAsync(ct);
        return categoria.ADto();
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var categoria = await _categorias.ObtenerPorIdAsync(id, ct)
            ?? throw new NoEncontradoException("la categoría", id);

        if (await _categorias.TieneProductosAsync(id, ct))
            throw new ConflictoException("No se puede eliminar una categoría que tiene productos.", "CATEGORIA_CON_PRODUCTOS");

        _categorias.Eliminar(categoria);
        await _uow.GuardarCambiosAsync(ct);
    }
}
