using AMSoluciones.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMSoluciones.Infrastructure.Persistencia.Configuraciones;

public class VentaConfiguracion : IEntityTypeConfiguration<Venta>
{
    public void Configure(EntityTypeBuilder<Venta> builder)
    {
        builder.ToTable("Ventas", t =>
        {
            t.HasCheckConstraint("CK_Ventas_Total", "[Total] > 0");
            t.HasCheckConstraint("CK_Ventas_Estado",
                "([Estado] = 'Confirmada' AND [AnuladaUtc] IS NULL) OR " +
                "([Estado] = 'Anulada' AND [AnuladaUtc] IS NOT NULL AND [AnuladaUtc] >= [FechaUtc])");
            t.HasCheckConstraint("CK_Ventas_Utc",
                "DATEPART(TZOFFSET, [FechaUtc]) = 0 AND ([AnuladaUtc] IS NULL OR DATEPART(TZOFFSET, [AnuladaUtc]) = 0)");
        });

        builder.HasKey(v => v.Id);

        builder.Property(v => v.FechaUtc).HasColumnType("datetimeoffset(0)");
        builder.Property(v => v.AnuladaUtc).HasColumnType("datetimeoffset(0)");
        builder.Property(v => v.Total).HasPrecision(18, 2);

        // El enum se guarda como texto: 'Confirmada' o 'Anulada'.
        builder.Property(v => v.Estado)
            .HasConversion<string>()
            .HasMaxLength(12)
            .IsUnicode(false)
            .IsRequired();

        builder.Property(v => v.VersionFila).IsRowVersion();

        builder.HasIndex(v => new { v.Estado, v.FechaUtc });

        // Composición: los detalles pertenecen a la venta.
        builder.HasMany(v => v.Detalles)
            .WithOne()
            .HasForeignKey(d => d.VentaId)
            .OnDelete(DeleteBehavior.Cascade);

        // La colección pública es de solo lectura; EF usa el campo privado _detalles.
        builder.Navigation(v => v.Detalles)
            .HasField("_detalles")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
