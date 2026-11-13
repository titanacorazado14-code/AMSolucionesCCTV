using System.Globalization;
using AMSoluciones.Application;
using AMSoluciones.Infrastructure;
using AMSoluciones.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Authentication.Cookies;

// Cultura invariante: los números y decimales se leen siempre con punto (1450.00).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

// La Web usa las mismas capas que la API: llama directo a los casos de uso de Application.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Sesión por cookie. El login valida con ASP.NET Core Identity (IAutenticacionService).
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opciones =>
    {
        opciones.LoginPath = "/Login";
        opciones.AccessDeniedPath = "/AccesoDenegado";
        opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
        opciones.SlidingExpiration = true;
        opciones.Cookie.HttpOnly = true;
    });

builder.Services.AddAuthorization();

builder.Services.AddRazorPages(opciones =>
{
    // Todas las páginas exigen sesión, salvo estas.
    opciones.Conventions.AuthorizeFolder("/");
    opciones.Conventions.AllowAnonymousToPage("/Login");
    opciones.Conventions.AllowAnonymousToPage("/AccesoDenegado");
    opciones.Conventions.AllowAnonymousToPage("/Error");
});

var app = builder.Build();

// Aplica migraciones y carga datos iniciales si hace falta (no hace nada si ya existen).
await app.Services.InicializarBaseDatosAsync();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
