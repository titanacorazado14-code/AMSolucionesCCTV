using AMSoluciones.Domain.Entidades;
using AMSoluciones.Domain.Excepciones;
using AMSoluciones.Domain.Repositorios;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace AMSoluciones.Infrastructure.Persistencia;

/// <summary>
/// Contexto de Entity Framework. Hereda de IdentityDbContext (tablas de usuarios y roles)
/// e implementa IUnitOfWork (definida en Domain) para guardar todo en una transacción.
/// </summary>
public class AppDbContext : IdentityDbContext<IdentityUser>, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Inventario> Inventarios => Set<Inventario>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetallesVenta => Set<DetalleVenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Carga todas las clases IEntityTypeConfiguration de este proyecto.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    /// <summary>
    /// SaveChanges ya es una transacción: o se guarda todo o no se guarda nada.
    /// Aquí además se traducen los errores de SQL Server a excepciones de Domain.
    /// </summary>
    public async Task<int> GuardarCambiosAsync(CancellationToken ct = default)
    {
        try
        {
            return await SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // rowversion: otra operación cambió la misma fila. Se revierte todo.
            throw new ConcurrenciaException();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ConflictoException("Ya existe un registro con ese valor único.", "DUPLICADO");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 547 })
        {
            throw new ConflictoException("La operación choca con datos que dependen de este registro.", "DEPENDENCIAS");
        }
    }

    public async Task EjecutarEnTransaccionAsync(Func<Task> operacion, CancellationToken ct = default)
    {
        await using var transaccion = await Database.BeginTransactionAsync(ct);
        await operacion();
        await transaccion.CommitAsync(ct);   // si algo falla antes, al salir se hace rollback
    }
}
