using AMSoluciones.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMSoluciones.Infrastructure.Persistencia.Configuraciones;

public class DetalleVentaConfiguracion : IEntityTypeConfiguration<DetalleVenta>
{
    public void Configure(EntityTypeBuilder<DetalleVenta> builder)
    {
        builder.ToTable("DetallesVenta", t =>
        {
            t.HasCheckConstraint("CK_DetallesVenta_Nombre", "LEN(TRIM([ProductoNombre])) > 0");
            t.HasCheckConstraint("CK_DetallesVenta_Cantidad", "[Cantidad] BETWEEN 1 AND 1000000");
            t.HasCheckConstraint("CK_DetallesVenta_Precio", "[PrecioUnitario] > 0");
        });

        builder.HasKey(d => d.Id);

        builder.Property(d => d.ProductoNombre).IsRequired().HasMaxLength(Producto.LongitudMaximaNombre);
        builder.Property(d => d.PrecioUnitario).HasPrecision(8, 2);

        // SQL Server calcula el subtotal de cada línea.
        builder.Property(d => d.Subtotal)
            .HasPrecision(18, 2)
            .HasComputedColumnSql("CONVERT(decimal(18,2), [PrecioUnitario] * [Cantidad])", stored: true);

        // Un producto no puede repetirse dentro de la misma venta.
        builder.HasIndex(d => new { d.VentaId, d.ProductoId }).IsUnique();

        builder.HasOne<Producto>()
            .WithMany()
            .HasForeignKey(d => d.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
