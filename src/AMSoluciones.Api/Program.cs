using System.Text;
using AMSoluciones.Api.Errores;
using AMSoluciones.Api.Seguridad;
using AMSoluciones.Application;
using AMSoluciones.Infrastructure;
using AMSoluciones.Infrastructure.Persistencia;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Capas: Api solo conoce Application e Infrastructure a través de estos dos métodos.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---- JWT ----
builder.Services.Configure<JwtOpciones>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<JwtService>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOpciones>()
    ?? throw new InvalidOperationException("Falta la sección 'Jwt' en appsettings.json.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

// ---- CORS: solo el origen exacto del frontend, con los métodos y encabezados del contrato ----
var origenes = builder.Configuration.GetSection("Cors:OrigenesPermitidos").Get<string[]>() ?? [];
builder.Services.AddCors(opciones =>
    opciones.AddDefaultPolicy(politica => politica
        .WithOrigins(origenes)
        .WithMethods("GET", "POST", "PUT", "DELETE")
        .WithHeaders("Authorization", "Content-Type")));

builder.Services.AddControllers();

// ---- Errores: toda excepción se convierte en ProblemDetails con un código estable ----
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorExcepciones>();

var app = builder.Build();

// Aplica migraciones y carga roles, usuarios y datos iniciales.
await app.Services.InicializarBaseDatosAsync();

app.UseExceptionHandler();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
