namespace AMSoluciones.Api.Seguridad;

/// <summary>Configuración del JWT (sección "Jwt" de appsettings.json).</summary>
public class JwtOpciones
{
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int MinutosExpiracion { get; set; } = 60;
}
