using AMSoluciones.Application.Abstracciones;
using AMSoluciones.Domain.Repositorios;
using AMSoluciones.Infrastructure.Persistencia;
using AMSoluciones.Infrastructure.Persistencia.Repositorios;
using AMSoluciones.Infrastructure.Seguridad;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AMSoluciones.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra todo lo de Infrastructure: base de datos, repositorios e Identity.
    /// Api y Web solo llaman a este método.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cadena = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'DefaultConnection'.");

        services.AddDbContext<AppDbContext>(opciones => opciones.UseSqlServer(cadena));

        // El mismo contexto actúa como unidad de trabajo.
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<ICategoriaRepository, CategoriaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<IInventarioRepository, InventarioRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();

        services
            .AddIdentityCore<IdentityUser>(opciones =>
            {
                opciones.Password.RequiredLength = 8;
                opciones.Password.RequireDigit = true;
                opciones.Password.RequireUppercase = true;
                opciones.Password.RequireLowercase = true;
                opciones.Password.RequireNonAlphanumeric = true;
                opciones.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IAutenticacionService, AutenticacionService>();

        return services;
    }
}
