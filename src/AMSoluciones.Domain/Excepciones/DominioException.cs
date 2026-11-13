namespace AMSoluciones.Domain.Excepciones;

/// <summary>
/// Excepción base para toda regla de negocio violada.
/// La API la traduce a HTTP 400 (solicitud inválida).
/// El código es estable: sirve para que el cliente reconozca el error.
/// </summary>
public class DominioException : Exception
{
    public string Codigo { get; }

    public DominioException(string mensaje, string codigo = "DOMINIO_INVALIDO") : base(mensaje)
    {
        Codigo = codigo;
    }
}
