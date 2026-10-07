using Cognexa.Application.Anotacoes;
using Cognexa.Domain.Anotacoes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace Cognexa.Infrastructure.Persistencia;

public sealed class ControleDeConteudo(CognexaDbContext db, IOptions<ConteudoOptions> options, TimeProvider tempo) : IControleDeConteudo
{
    public async Task<T> ExecutarAsync<T>(Guid idUsuario, Guid idLivro, string? trecho, int? pagina, Func<Task<T>> acao, CancellationToken ct)
    {
        options.Value.Validar();
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // Todas as capturas da obra compartilham o lock, incluindo solicitações concorrentes.
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({idLivro.ToString()}, 0))", ct);
        if (!string.IsNullOrWhiteSpace(trecho))
        {
            var agora = tempo.GetUtcNow();
            var desde = agora.AddHours(-options.Value.JanelaHoras);
            var registros = await db.Set<RegistroDeConteudo>().Where(x => x.IdLivro == idLivro && x.Data >= desde).ToListAsync(ct);
            PoliticaDeConteudo.Validar(options.Value, trecho.Length, pagina, registros);
            db.Add(new RegistroDeConteudo(idUsuario, idLivro, trecho.Length, pagina, agora));
        }
        var result = await acao();
        await transaction.CommitAsync(ct);
        return result;
    }
}
public sealed class RegistroDeConteudoConfiguration : IEntityTypeConfiguration<RegistroDeConteudo>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<RegistroDeConteudo> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => new { x.IdLivro, x.Data });
    }
}
public sealed class DenunciaConfiguration : IEntityTypeConfiguration<Denuncia>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Denuncia> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.HasIndex(x => new { x.IdAnotacao, x.IdDenunciante }).IsUnique();
    }
}
public sealed class RegistroDePublicacaoConfiguration : IEntityTypeConfiguration<RegistroDePublicacao>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<RegistroDePublicacao> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.Obra).HasMaxLength(500);
        b.Property(x => x.Autor).HasMaxLength(500);
        b.HasIndex(x => new { x.IdAnotacao, x.Data });
    }
}
