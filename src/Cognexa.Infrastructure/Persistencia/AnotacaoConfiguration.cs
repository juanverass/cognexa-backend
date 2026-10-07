using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Biblioteca;
using Cognexa.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Cognexa.Infrastructure.Persistencia;

public sealed class AnotacaoConfiguration : IEntityTypeConfiguration<Anotacao>
{
    public void Configure(EntityTypeBuilder<Anotacao> b)
    {
        b.Property(x => x.Obra).HasMaxLength(500);
        b.Property(x => x.Autor).HasMaxLength(500);
        b.Property<uint>("Versao").IsRowVersion();
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.TrechoOriginal).HasMaxLength(20000);
        b.Property(x => x.Comentario).HasMaxLength(20000);
        b.Property(x => x.Localizacao).HasMaxLength(500);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Livro>().WithMany().HasForeignKey(x => new { x.IdLivro, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Capitulo>().WithMany().HasForeignKey(x => new { x.IdCapitulo, x.IdLivro, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdLivro, x.IdUsuario }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdUsuario, x.IdLivro, x.Tipo, x.Pagina });
    }
}
