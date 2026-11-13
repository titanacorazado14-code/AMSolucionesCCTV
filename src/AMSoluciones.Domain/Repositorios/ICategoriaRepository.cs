using AMSoluciones.Domain.Entidades;

namespace AMSoluciones.Domain.Repositorios;

public interface ICategoriaRepository
{
    Task<Categoria?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Categoria>> ListarAsync(CancellationToken ct = default);
    Task<bool> ExistePorNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default);
    Task<bool> TieneProductosAsync(int categoriaId, CancellationToken ct = default);
    Task AgregarAsync(Categoria categoria, CancellationToken ct = default);
    void Eliminar(Categoria categoria);
}
