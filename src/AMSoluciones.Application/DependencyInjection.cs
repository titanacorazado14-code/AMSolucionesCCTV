using AMSoluciones.Application.CasosDeUso;
using AMSoluciones.Application.Servicios;
using Microsoft.Extensions.DependencyInjection;

namespace AMSoluciones.Application;

public static class DependencyInjection
{
    /// <summary>Registra los servicios y casos de uso de la capa Application.</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CategoriaService>();
        services.AddScoped<ProductoService>();
        services.AddScoped<InventarioService>();
        services.AddScoped<VentaService>();
        services.AddScoped<DashboardService>();

        services.AddScoped<RegistrarVentaUseCase>();
        services.AddScoped<ReponerInventarioUseCase>();
        services.AddScoped<AnularVentaUseCase>();

        return services;
    }
}
