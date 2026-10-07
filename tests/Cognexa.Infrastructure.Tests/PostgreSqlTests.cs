using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Biblioteca;
using Cognexa.Domain.Compartilhado;
using Cognexa.Domain.Conhecimento;
using Cognexa.Domain.Usuarios;
using Cognexa.Infrastructure.Persistencia;
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
