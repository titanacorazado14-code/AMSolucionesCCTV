using AMSoluciones.Infrastructure.Persistencia;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AMSoluciones.Infrastructure.Tests;

/// <summary>
/// Crea una base de datos SQL Server temporal para una prueba y la elimina al terminar.
/// Usa la variable de entorno TF04_TEST_CONNECTION si existe (por ejemplo, con usuario y contraseña);
/// si no, se conecta a localhost,1433 con autenticación de Windows.
/// </summary>
public sealed class BaseDatosPrueba : IAsyncDisposable
{
    public DbContextOptions<AppDbContext> Opciones { get; }

    private BaseDatosPrueba(DbContextOptions<AppDbContext> opciones)
    {
        Opciones = opciones;
    }

    public static async Task<BaseDatosPrueba> CrearAsync()
    {
        var cadenaBase = Environment.GetEnvironmentVariable("TF04_TEST_CONNECTION")
            ?? "Server=localhost,1433;Trusted_Connection=True;TrustServerCertificate=True;";

        var cadena = new SqlConnectionStringBuilder(cadenaBase)
        {
            InitialCatalog = "TF04_Pruebas_" + Guid.NewGuid().ToString("N")
        };

        var opciones = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(cadena.ConnectionString)
            .Options;

        var bd = new BaseDatosPrueba(opciones);
        await using var db = bd.Crear();
        await db.Database.EnsureCreatedAsync();
        return bd;
    }

    public AppDbContext Crear() => new(Opciones);

    public async ValueTask DisposeAsync()
    {
        await using var db = Crear();
        await db.Database.EnsureDeletedAsync();
    }
}
