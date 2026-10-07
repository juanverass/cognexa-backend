using Cognexa.Domain.Conhecimento;
using Cognexa.Domain.Revisoes;
using Cognexa.Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Cognexa.Infrastructure.Persistencia;

public sealed class PerguntaConfiguration : IEntityTypeConfiguration<PerguntaDeRevisao>
{
    public void Configure(EntityTypeBuilder<PerguntaDeRevisao> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Pergunta).HasMaxLength(4000).IsRequired();
        b.Property(x => x.RespostaEsperada).HasMaxLength(10000).IsRequired();
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Aprendizado>().WithMany().HasForeignKey(x => new { x.IdAprendizado, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdUsuario, x.IdAprendizado });
    }
}
public sealed class RevisaoConfiguration : IEntityTypeConfiguration<Revisao>
{
    public void Configure(EntityTypeBuilder<Revisao> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property<uint>("xmin").IsRowVersion();
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<PerguntaDeRevisao>().WithOne().HasForeignKey<Revisao>(x => new { x.IdPergunta, x.IdUsuario }).HasPrincipalKey<PerguntaDeRevisao>(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdUsuario, x.ProximaRevisao });
    }
}
public sealed class HistoricoConfiguration : IEntityTypeConfiguration<HistoricoDeRevisao>
{
    public void Configure(EntityTypeBuilder<HistoricoDeRevisao> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Resposta).HasMaxLength(10000);
        b.HasOne<Usuario>().WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Revisao>().WithMany().HasForeignKey(x => new { x.IdRevisao, x.IdUsuario }).HasPrincipalKey(x => new { x.Id, x.IdUsuario }).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.IdUsuario, x.IdRevisao, x.Data });
    }
}
