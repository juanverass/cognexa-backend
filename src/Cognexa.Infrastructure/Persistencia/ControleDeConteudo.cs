using Cognexa.Application.Anotacoes;
using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Biblioteca;
using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Compartilhado;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace Cognexa.Infrastructure.Persistencia;

public sealed class ControleDeConteudo(CognexaDbContext db, IOptions<ConteudoOptions> options, TimeProvider tempo) : IControleDeConteudo
{
    public async Task<T> ExecutarAsync<T>(Guid idUsuario, Guid idLivro, string? trecho, int? pagina, Func<Task<T>> acao, CancellationToken ct)
    {
        options.Value.Validar();
        var chave = await ExigirChaveAsync(idUsuario, idLivro, ct);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        // Todas as capturas da obra compartilham o lock, incluindo solicitações concorrentes.
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({chave}, 0))", ct);
        if (!string.IsNullOrWhiteSpace(trecho))
        {
            var agora = tempo.GetUtcNow();
            var desde = agora.AddHours(-options.Value.JanelaHoras);
            var registros = await db.Set<RegistroDeConteudo>().Where(x => x.IdUsuario == idUsuario && (x.ChaveDaObra == chave || x.ChaveDaObra == ChaveDeObra.LegadoIndeterminado) && x.Data >= desde).ToListAsync(ct);
            PoliticaDeConteudo.Validar(options.Value, trecho.Length, pagina, registros);
            db.Add(new RegistroDeConteudo(idUsuario, idLivro, trecho.Length, pagina, agora, chave));
        }
        var result = await acao();
        await transaction.CommitAsync(ct);
        return result;
    }
    private async Task<string> ExigirChaveAsync(Guid usuario, Guid livro, CancellationToken ct) =>
        await db.Set<Livro>().Where(x => x.Id == livro && x.IdUsuario == usuario).Select(x => x.ChaveDaObra).SingleOrDefaultAsync(ct) ?? throw new NaoEncontradoException();
    public async Task ValidarPublicacaoAsync(Guid idUsuario, Guid idLivro, Guid idAnotacao, string? trecho, CancellationToken ct)
    {
        options.Value.Validar();
        var chave = await ExigirChaveAsync(idUsuario, idLivro, ct);
        var total = await (from anotacao in db.Set<Anotacao>()
                           join livro in db.Set<Livro>() on anotacao.IdLivro equals livro.Id
                           where livro.ChaveDaObra == chave && anotacao.Id != idAnotacao &&
                               (anotacao.Publicacao == EstadoDePublicacao.Publicado || anotacao.Publicacao == EstadoDePublicacao.Publicavel)
                           select (long)(anotacao.TrechoOriginal == null ? 0 : anotacao.TrechoOriginal.Length)).SumAsync(ct);
        if ((trecho?.Length ?? 0) > options.Value.MaximoPublicoPorTrecho || total + (trecho?.Length ?? 0) > options.Value.MaximoPublicoPorLivro)
            throw new RegraDeDominioException("Limite técnico público excedido.");
    }
}
public sealed class RegistroDeConteudoConfiguration : IEntityTypeConfiguration<RegistroDeConteudo>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<RegistroDeConteudo> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedNever();
        b.Property(x => x.ChaveDaObra).HasMaxLength(64).IsRequired();
        b.HasIndex(x => new { x.IdUsuario, x.ChaveDaObra, x.Data });
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
