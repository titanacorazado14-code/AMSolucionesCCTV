using AMSoluciones.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMSoluciones.Infrastructure.Persistencia.Configuraciones;

public class InventarioConfiguracion : IEntityTypeConfiguration<Inventario>
{
    public void Configure(EntityTypeBuilder<Inventario> builder)
    {
        builder.ToTable("Inventarios", t =>
        {
            t.HasCheckConstraint("CK_Inventarios_Disponible", "[Disponible] BETWEEN 0 AND 1000000");
        });

        // La clave primaria es el propio ProductoId (una fila por producto).
        builder.HasKey(i => i.ProductoId);
        builder.Property(i => i.ProductoId).ValueGeneratedNever();

        // Control de concurrencia optimista: rowversion.
        builder.Property(i => i.VersionFila).IsRowVersion();

        builder.HasOne(i => i.Producto)
            .WithOne()
            .HasForeignKey<Inventario>(i => i.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
