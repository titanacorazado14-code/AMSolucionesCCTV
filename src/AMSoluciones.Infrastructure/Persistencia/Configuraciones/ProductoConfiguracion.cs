using AMSoluciones.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMSoluciones.Infrastructure.Persistencia.Configuraciones;

public class ProductoConfiguracion : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos", t =>
        {
            t.HasCheckConstraint("CK_Productos_Precio", "[Precio] > 0");
            t.HasCheckConstraint("CK_Productos_Nombre", "LEN(TRIM([Nombre])) > 0");
            t.HasCheckConstraint("CK_Productos_Sku",
                "LEN([Sku]) > 0 AND [Sku] COLLATE Latin1_General_100_BIN2 NOT LIKE '%[^A-Z0-9-]%'");
        });

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku)
            .IsRequired()
            .HasMaxLength(Producto.LongitudMaximaSku)
            .IsUnicode(false)
            .UseCollation("Latin1_General_100_CI_AS");

        builder.Property(p => p.Nombre).IsRequired().HasMaxLength(Producto.LongitudMaximaNombre);
        builder.Property(p => p.Precio).HasPrecision(8, 2);

        builder.HasIndex(p => p.Sku).IsUnique();

        // Un producto pertenece a una categoría; no se puede borrar una categoría con productos.
        builder.HasOne(p => p.Categoria)
            .WithMany()
            .HasForeignKey(p => p.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
