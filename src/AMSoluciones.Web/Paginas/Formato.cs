using System.Globalization;

namespace AMSoluciones.Web.Paginas;

/// <summary>Formato de importes y fechas para mostrar en pantalla.</summary>
public static class Formato
{
    public static string Moneda(decimal valor) =>
        "RD$ " + valor.ToString("N2", CultureInfo.InvariantCulture);

    public static string Fecha(DateTimeOffset fecha) =>
        fecha.ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
}
