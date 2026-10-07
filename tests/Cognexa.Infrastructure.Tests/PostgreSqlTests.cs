using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Biblioteca;
using Cognexa.Domain.Compartilhado;
using Cognexa.Domain.Conhecimento;
using Cognexa.Domain.Revisoes;
using Cognexa.Domain.Usuarios;
using Cognexa.Infrastructure.Persistencia;
using Cognexa.Infrastructure.Revisoes;
using Cognexa.Tests.Compartilhado;
using Microsoft.EntityFrameworkCore;
namespace Cognexa.Infrastructure.Tests;

public class PostgreSqlTests(PostgreSqlFixture banco) : IClassFixture<PostgreSqlFixture>
{
    [PostgreSqlFact]
    public async Task PersisteAgregadosEImpedeExclusaoDeFonteReferenciada()
    {
        await using var db = banco.Contexto();
        var usuario = new Usuario("Teste");
        var livro = new Livro(usuario.Id, "Livro", ["Autor"], 100);
        var anotacao = new Anotacao(usuario.Id, livro.Id, null, TipoDeAnotacao.Citacao, "Original", "Comentário", 10, null, DateTimeOffset.UtcNow);
        var aprendizado = new Aprendizado(usuario.Id, "Síntese", [(anotacao.Id, livro.Id)], [], DateTimeOffset.UtcNow);
        db.AddRange(usuario, livro, anotacao, aprendizado);
        await db.SalvarAsync();
        db.ChangeTracker.Clear();
        var salvo = await new Repository<Aprendizado>(db).ObterAsync(aprendizado.Id);
        Assert.Equal(anotacao.Id, salvo!.Fontes.Single().IdAnotacao);
        var livroSalvo = await new Repository<Livro>(db).ObterAsync(livro.Id);
        Assert.Equal("Autor", livroSalvo!.Autores.Single().Nome);
        livroSalvo.Atualizar("Novo título", ["Outro autor"], 100, null, null, null);
        await db.SalvarAsync();
        db.ChangeTracker.Clear();
        Assert.Equal("Outro autor", (await new Repository<Livro>(db).ObterAsync(livro.Id))!.Autores.Single().Nome);
        var fonte = await new Repository<Anotacao>(db).ObterAsync(anotacao.Id);
        db.Remove(fonte!);
        await Assert.ThrowsAsync<ConflitoException>(() => db.SalvarAsync());
        await using var novo = banco.Contexto();
        Assert.Equal("Original", (await novo.Set<Anotacao>().FindAsync(anotacao.Id))!.TrechoOriginal);
    }
    [PostgreSqlFact]
    public async Task ConsultaDevidasPorLivroEConceitoEMantemHistorico()
    {
        await using var db = banco.Contexto();
        var usuario = new Usuario("Teste");
        var outro = new Usuario("Outro");
        var livro = new Livro(usuario.Id, "Livro", ["Autor"]);
        var conceito = new Conceito(usuario.Id, "Tema");
        var nota = new Anotacao(usuario.Id, livro.Id, null, TipoDeAnotacao.Insight, null, "Nota", null, null, DateTimeOffset.UtcNow);
        var aprendizado = new Aprendizado(usuario.Id, "Síntese", [(nota.Id, livro.Id)], [conceito.Id], DateTimeOffset.UtcNow);
        var pergunta = new PerguntaDeRevisao(usuario.Id, aprendizado.Id, "Pergunta?", "Resposta");
        var agora = DateTimeOffset.UtcNow;
        var revisao = new Revisao(usuario.Id, pergunta.Id, agora.AddHours(-1));
        db.AddRange(usuario, outro, livro, conceito, nota, aprendizado, pergunta, revisao);
        await db.SalvarAsync();
        db.ChangeTracker.Clear();
        var repository = new RevisaoRepository(db);
        Assert.Single(await repository.ListarDevidasAsync(usuario.Id, agora, livro.Id, conceito.Id, 100, default));
        Assert.Empty(await repository.ListarDevidasAsync(outro.Id, agora, null, null, 100, default));
        Assert.Empty(await repository.ListarDevidasAsync(usuario.Id, agora, Guid.NewGuid(), null, 100, default));
        var entidade = await repository.ObterAsync(revisao.Id);
        db.Add(entidade!.Registrar(ResultadoDaRevisao.Bom, "Resposta", agora, agora.AddDays(1)));
        await db.SalvarAsync();
        Assert.Empty(await repository.ListarDevidasAsync(usuario.Id, agora, null, null, 100, default));
        Assert.Single(await repository.ListarHistoricoAsync(usuario.Id, revisao.Id, 1, default));
        Assert.Empty(await repository.ListarHistoricoAsync(outro.Id, revisao.Id, 1, default));
        await using var concorrente = banco.Contexto();
        var copia = await concorrente.Set<Revisao>().SingleAsync(x => x.Id == revisao.Id);
        var futuro = agora.AddDays(1);
        db.Add(entidade.Registrar(ResultadoDaRevisao.Bom, "Resposta", futuro, futuro.AddDays(1)));
        await db.SalvarAsync();
        concorrente.Add(copia.Registrar(ResultadoDaRevisao.Errou, null, futuro, futuro.AddDays(1)));
        await Assert.ThrowsAsync<ConflitoException>(() => concorrente.SalvarAsync());
        Assert.Equal(2, (await repository.ListarHistoricoAsync(usuario.Id, revisao.Id, 1, default)).Count);
    }
    [PostgreSqlFact]
    public async Task BancoRejeitaReferenciasEntreProprietarios()
    {
        await using var db = banco.Contexto();
        var usuario = new Usuario("Teste");
        var outro = new Usuario("Outro");
        var livro = new Livro(usuario.Id, "Livro", ["Autor"]);
        var conceitoAlheio = new Conceito(outro.Id, "Privado");
        var anotacao = new Anotacao(usuario.Id, livro.Id, null, TipoDeAnotacao.Insight, null, "Nota", null, null, DateTimeOffset.UtcNow);
        db.AddRange(usuario, outro, livro, conceitoAlheio, anotacao);
        await db.SalvarAsync();
        var aprendizado = new Aprendizado(usuario.Id, "Síntese", [(anotacao.Id, livro.Id)], [conceitoAlheio.Id], DateTimeOffset.UtcNow);
        db.Add(aprendizado);
        await Assert.ThrowsAsync<ConflitoException>(() => db.SalvarAsync());
        await using var separado = banco.Contexto();
        separado.Add(new Anotacao(outro.Id, livro.Id, null, TipoDeAnotacao.Insight, null, "Não permitido", null, null, DateTimeOffset.UtcNow));
        await Assert.ThrowsAsync<ConflitoException>(() => separado.SalvarAsync());
        await using var verificado = banco.Contexto();
        Assert.False(await verificado.Set<Aprendizado>().AnyAsync(x => x.Id == aprendizado.Id));
    }
}
