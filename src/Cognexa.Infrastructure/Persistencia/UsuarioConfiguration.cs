using Cognexa.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Cognexa.Infrastructure.Persistencia;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Nome).HasMaxLength(200).IsRequired();
        builder.OwnsOne(x => x.Preferencias, owned => { owned.Property(x => x.Idioma).HasMaxLength(20).IsRequired(); });
        builder.Navigation(x => x.Preferencias).IsRequired();
    }
}
