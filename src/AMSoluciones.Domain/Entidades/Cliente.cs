using AMSoluciones.Domain.Comun;
using AMSoluciones.Domain.Excepciones;

namespace AMSoluciones.Domain.Entidades;

public class Cliente
{
    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string? Telefono { get; private set; }
    public string? Direccion { get; private set; }
    public string? Email { get; private set; }

    // Constructor vacío solo para Entity Framework.
    private Cliente()
    {
    }

    public Cliente(string nombre, string? telefono = null, string? direccion = null, string? email = null)
    {
        Aplicar(nombre, telefono, direccion, email);
    }

    public void Actualizar(string nombre, string? telefono, string? direccion, string? email)
    {
        Aplicar(nombre, telefono, direccion, email);
    }

    private void Aplicar(string nombre, string? telefono, string? direccion, string? email)
    {
        Nombre = Validar.TextoObligatorio(nombre, "Nombre", 120);
        Telefono = Validar.TextoOpcional(telefono, "Teléfono", 30);
        Direccion = Validar.TextoOpcional(direccion, "Dirección", 250);
        Email = Validar.TextoOpcional(email, "Email", 120);

        if (Email is not null && !Email.Contains('@'))
            throw new DominioException("El email del cliente no es válido.");
    }
}
