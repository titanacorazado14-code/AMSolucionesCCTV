using AMSoluciones.Domain.Entidades;

namespace AMSoluciones.Domain.Repositorios;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken ct = default);
    Task AgregarAsync(Cliente cliente, CancellationToken ct = default);
}
