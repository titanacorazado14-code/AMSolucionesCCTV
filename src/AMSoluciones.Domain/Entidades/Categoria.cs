using AMSoluciones.Domain.Comun;

namespace AMSoluciones.Domain.Entidades;

public class Categoria
{
    public const int LongitudMaximaNombre = 100;

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;

    // Constructor vacío solo para Entity Framework.
    private Categoria()
    {
    }

    public Categoria(string nombre)
    {
        Nombre = Validar.TextoObligatorio(nombre, "Nombre", LongitudMaximaNombre);
    }

    public void Renombrar(string nombre)
    {
        Nombre = Validar.TextoObligatorio(nombre, "Nombre", LongitudMaximaNombre);
    }
}
