using Cognexa.Domain.Biblioteca;
using Cognexa.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Cognexa.Infrastructure.Persistencia;

public sealed class LivroConfiguration : IEntityTypeConfiguration<Livro>
{
    public void Configure(EntityTypeBuilder<Livro> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Titulo).HasMaxLength(300).IsRequired();
        b.Property(x => x.Isbn).HasMaxLength(30);
        b.Property(x => x.Edicao).HasMaxLength(100);
        b.Property(x => x.Capa).HasMaxLength(2000);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdUsuario);
        b.HasMany(x => x.Autores).WithOne().HasForeignKey(x => x.IdLivro).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Autores).AutoInclude();
    }
}
public sealed class AutorConfiguration : IEntityTypeConfiguration<Autor>
{
    public void Configure(EntityTypeBuilder<Autor> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Nome).HasMaxLength(200).IsRequired();
    }
}
public sealed class LeituraConfiguration : IEntityTypeConfiguration<Leitura>
{
    public void Configure(EntityTypeBuilder<Leitura> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Motivo).HasMaxLength(2000);
        b.HasOne<Livro>().WithOne().HasForeignKey<Leitura>(x => new { x.IdLivro, x.IdUsuario }).HasPrincipalKey<Livro>(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdUsuario, x.Status });
    }
}
public sealed class CapituloConfiguration : IEntityTypeConfiguration<Capitulo>
{
    public void Configure(EntityTypeBuilder<Capitulo> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Titulo).HasMaxLength(300).IsRequired();
        b.HasOne<Livro>().WithMany().HasForeignKey(x => new { x.IdLivro, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdLivro, x.Ordem }).IsUnique();
    }
}
