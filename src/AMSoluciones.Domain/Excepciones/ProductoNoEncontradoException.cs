namespace AMSoluciones.Domain.Excepciones;

/// <summary>
/// Se lanza cuando un caso de uso referencia un producto que no existe.
/// La API la traducirá a un error 404.
/// </summary>
public class ProductoNoEncontradoException : DominioException
{
    public int ProductoId { get; }

    public ProductoNoEncontradoException(int productoId)
        : base($"No existe el producto con id {productoId}.")
    {
        ProductoId = productoId;
    }
}
