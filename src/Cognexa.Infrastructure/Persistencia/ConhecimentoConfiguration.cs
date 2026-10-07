using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Biblioteca;
using Cognexa.Domain.Conhecimento;
using Cognexa.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Cognexa.Infrastructure.Persistencia;

public sealed class AprendizadoConfiguration : IEntityTypeConfiguration<Aprendizado>
{
    public void Configure(EntityTypeBuilder<Aprendizado> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Conteudo).HasMaxLength(20000).IsRequired();
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => x.IdUsuario);
        b.HasMany(x => x.Fontes).WithOne().HasForeignKey(x => new { x.IdAprendizado, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.Conceitos).WithOne().HasForeignKey(x => new { x.IdAprendizado, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(x => x.Fontes).AutoInclude();
        b.Navigation(x => x.Conceitos).AutoInclude();
    }
}
public sealed class FonteConfiguration : IEntityTypeConfiguration<FonteDoAprendizado>
{
    public void Configure(EntityTypeBuilder<FonteDoAprendizado> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasOne<Anotacao>().WithMany().HasForeignKey(x => new { x.IdAnotacao, x.IdLivro, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdLivro, x.IdUsuario }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Livro>().WithMany().HasForeignKey(x => new { x.IdLivro, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdAprendizado, x.IdAnotacao }).IsUnique();
    }
}
public sealed class ConceitoConfiguration : IEntityTypeConfiguration<Conceito>
{
    public void Configure(EntityTypeBuilder<Conceito> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Nome).HasMaxLength(200).IsRequired();
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdUsuario, x.Nome }).IsUnique();
    }
}
public sealed class ConceitoDoAprendizadoConfiguration : IEntityTypeConfiguration<ConceitoDoAprendizado>
{
    public void Configure(EntityTypeBuilder<ConceitoDoAprendizado> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasOne<Conceito>().WithMany().HasForeignKey(x => new { x.IdConceito, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdAprendizado, x.IdConceito }).IsUnique();
    }
}
public sealed class AplicacaoConfiguration : IEntityTypeConfiguration<AplicacaoPratica>
{
    public void Configure(EntityTypeBuilder<AplicacaoPratica> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Descricao).HasMaxLength(20000).IsRequired();
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Aprendizado>().WithMany().HasForeignKey(x => new { x.IdAprendizado, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdUsuario, x.IdAprendizado });
    }
}
public sealed class RelacaoConfiguration : IEntityTypeConfiguration<RelacaoEntreConceitos>
{
    public void Configure(EntityTypeBuilder<RelacaoEntreConceitos> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Conceito>().WithMany().HasForeignKey(x => new { x.IdConceitoOrigem, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Conceito>().WithMany().HasForeignKey(x => new { x.IdConceitoDestino, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdUsuario, x.IdConceitoOrigem, x.IdConceitoDestino, x.Tipo }).IsUnique();
    }
}
