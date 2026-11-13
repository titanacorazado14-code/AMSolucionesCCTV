using AMSoluciones.Domain.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AMSoluciones.Infrastructure.Persistencia.Configuraciones;

public class CategoriaConfiguracion : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias", t =>
        {
            t.HasCheckConstraint("CK_Categorias_Nombre", "LEN(TRIM([Nombre])) > 0");
        });

        builder.HasKey(c => c.Id);

        // Collation "CI" = sin distinguir mayúsculas: "Tecnología" y "TECNOLOGÍA" son la misma categoría.
        builder.Property(c => c.Nombre)
            .IsRequired()
            .HasMaxLength(Categoria.LongitudMaximaNombre)
            .UseCollation("Latin1_General_100_CI_AS");

        builder.HasIndex(c => c.Nombre).IsUnique();
    }
}
