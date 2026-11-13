namespace AMSoluciones.Domain.Excepciones;

/// <summary>
/// Se lanza cuando un caso de uso referencia algo que no existe. La API la traduce a HTTP 404.
/// </summary>
public class NoEncontradoException : DominioException
{
    public NoEncontradoException(string entidad, int id)
        : base($"No existe {entidad} con id {id}.", "NO_ENCONTRADO")
    {
    }
}
