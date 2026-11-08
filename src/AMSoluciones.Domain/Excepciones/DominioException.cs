namespace AMSoluciones.Domain.Excepciones;

/// <summary>
/// Excepción base para toda regla de negocio violada.
/// La API la traducirá a un error 400 (solicitud inválida).
/// </summary>
public class DominioException : Exception
{
    public DominioException(string mensaje) : base(mensaje)
    {
    }
}
