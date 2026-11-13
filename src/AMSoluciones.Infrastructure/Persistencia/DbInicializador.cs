using AMSoluciones.Domain.Constantes;
using AMSoluciones.Domain.Entidades;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AMSoluciones.Infrastructure.Persistencia;

/// <summary>
/// Al arrancar la aplicación: aplica las migraciones pendientes y carga los roles,
/// los usuarios de prueba y el catálogo inicial de AM Soluciones (productos CCTV).
/// Si los datos ya existen, no hace nada.
/// </summary>
public static class DbInicializador
{
    // Credenciales de DESARROLLO. Documentarlas en el README.
    public const string AdminEmail = "admin@amsoluciones.com";
    public const string AdminClave = "Admin123!";
    public const string GestorEmail = "gestor@amsoluciones.com";
    public const string GestorClave = "Gestor123!";

    public static async Task InicializarBaseDatosAsync(this IServiceProvider services)
    {
        // Las herramientas de EF (dotnet ef) ejecutan el arranque de la aplicación; ahí no se debe tocar la base.
        if (EF.IsDesignTime)
            return;

        using var scope = services.CreateScope();
        var proveedor = scope.ServiceProvider;

        var db = proveedor.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();

        await SembrarRolesYUsuariosAsync(proveedor);
        await SembrarCatalogoAsync(db);
    }

    private static async Task SembrarRolesYUsuariosAsync(IServiceProvider proveedor)
    {
        var roleManager = proveedor.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = proveedor.GetRequiredService<UserManager<IdentityUser>>();

        foreach (var rol in new[] { Roles.Administrador, Roles.Gestor })
        {
            if (!await roleManager.RoleExistsAsync(rol))
                await roleManager.CreateAsync(new IdentityRole(rol));
        }

        await CrearUsuarioAsync(userManager, AdminEmail, AdminClave, Roles.Administrador);
        await CrearUsuarioAsync(userManager, GestorEmail, GestorClave, Roles.Gestor);
    }

    private static async Task CrearUsuarioAsync(UserManager<IdentityUser> userManager, string email, string clave, string rol)
    {
        var usuario = await userManager.FindByEmailAsync(email);

        if (usuario is null)
        {
            usuario = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
            var resultado = await userManager.CreateAsync(usuario, clave);

            if (!resultado.Succeeded)
            {
                var errores = string.Join("; ", resultado.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"No se pudo crear el usuario {email}: {errores}");
            }
        }

        if (!await userManager.IsInRoleAsync(usuario, rol))
            await userManager.AddToRoleAsync(usuario, rol);
    }

    private static async Task SembrarCatalogoAsync(AppDbContext db)
    {
        if (await db.Categorias.AnyAsync())
            return;

        var categorias = new[]
        {
            new Categoria("Cámaras"),
            new Categoria("Grabadores DVR/NVR"),
            new Categoria("Almacenamiento"),
            new Categoria("Energía"),
            new Categoria("Cableado y conectores")
        };
        await db.Categorias.AddRangeAsync(categorias);
        await db.SaveChangesAsync();

        var idPorNombre = categorias.ToDictionary(c => c.Nombre, c => c.Id);

        // Sku, nombre, precio de venta (RD$), existencias netas actuales.
        var datos = new (string Categoria, string Sku, string Nombre, decimal Precio, int Stock)[]
        {
            ("Cámaras", "CAM-001", "Cámara bala 2MP 1080p", 1450.00m, 18),
            ("Cámaras", "CAM-002", "Cámara domo 2MP 1080p", 1400.00m, 15),
            ("Cámaras", "CAM-003", "Cámara IP bala 4MP PoE", 3900.00m, 8),
            ("Grabadores DVR/NVR", "DVR-001", "DVR 4 canales 1080p", 3800.00m, 6),
            ("Grabadores DVR/NVR", "NVR-001", "NVR 8 canales con PoE", 7200.00m, 4),
            ("Almacenamiento", "HDD-001", "Disco duro 1TB videovigilancia", 3600.00m, 10),
            ("Almacenamiento", "HDD-002", "Disco duro 2TB videovigilancia", 4800.00m, 8),
            ("Energía", "ENE-001", "Fuente de poder 12V 2A", 320.00m, 40),
            ("Cableado y conectores", "CON-001", "Conector BNC de compresión", 25.00m, 47),
            ("Cableado y conectores", "CON-002", "Par de balun pasivo HD", 180.00m, 50),
            ("Cableado y conectores", "CAB-001", "Rollo cable UTP Cat5e 305m", 5800.00m, 5)
        };

        var productos = datos
            .Select(d => new Producto(idPorNombre[d.Categoria], d.Sku, d.Nombre, d.Precio))
            .ToList();
        await db.Productos.AddRangeAsync(productos);
        await db.SaveChangesAsync();   // aquí se generan los Id

        var porSku = productos.ToDictionary(p => p.Sku);

        await db.Inventarios.AddRangeAsync(
            datos.Select(d => new Inventario(porSku[d.Sku].Id, d.Stock)));

        // Venta confirmada: 2 cámaras bala + 3 conectores BNC = RD$2,975.00
        var confirmada = new Venta(new DateTimeOffset(2026, 11, 1, 15, 0, 0, TimeSpan.Zero));
        confirmada.AgregarDetalle(porSku["CAM-001"].Id, porSku["CAM-001"].Nombre, 2, porSku["CAM-001"].Precio);
        confirmada.AgregarDetalle(porSku["CON-001"].Id, porSku["CON-001"].Nombre, 3, porSku["CON-001"].Precio);
        confirmada.Confirmar();

        // Venta anulada: 1 NVR + 1 disco de 2TB = RD$12,000.00 (sus unidades ya están en el inventario)
        var anulada = new Venta(new DateTimeOffset(2026, 11, 2, 15, 0, 0, TimeSpan.Zero));
        anulada.AgregarDetalle(porSku["NVR-001"].Id, porSku["NVR-001"].Nombre, 1, porSku["NVR-001"].Precio);
        anulada.AgregarDetalle(porSku["HDD-002"].Id, porSku["HDD-002"].Nombre, 1, porSku["HDD-002"].Precio);
        anulada.Confirmar();
        anulada.Anular(new DateTimeOffset(2026, 11, 2, 16, 0, 0, TimeSpan.Zero));

        await db.Ventas.AddRangeAsync(confirmada, anulada);
        await db.SaveChangesAsync();
    }
}
