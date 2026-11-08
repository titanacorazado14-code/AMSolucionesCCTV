using AMSoluciones.Domain.Comun;

namespace AMSoluciones.Domain.Entidades;

public class Categoria
{
    public const int LongitudMaximaNombre = 80;
    public const int LongitudMaximaDescripcion = 250;

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Descripcion { get; private set; }
    public bool Activa { get; private set; } = true;

    // Constructor vacío solo para Entity Framework.
    private Categoria()
    {
    }

    public Categoria(string nombre, string? descripcion = null)
    {
        Nombre = Validar.TextoObligatorio(nombre, "Nombre", LongitudMaximaNombre);
        Descripcion = Validar.TextoOpcional(descripcion, "Descripción", LongitudMaximaDescripcion);
    }

    public void Actualizar(string nombre, string? descripcion)
    {
        Nombre = Validar.TextoObligatorio(nombre, "Nombre", LongitudMaximaNombre);
        Descripcion = Validar.TextoOpcional(descripcion, "Descripción", LongitudMaximaDescripcion);
    }

    public void Activar() => Activa = true;

    public void Desactivar() => Activa = false;
}
