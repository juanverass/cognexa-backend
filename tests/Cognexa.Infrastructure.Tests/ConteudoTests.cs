using System.Net;
using System.Net.Http.Json;
using Cognexa.Application.Anotacoes;
using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Biblioteca;
using Cognexa.Domain.Compartilhado;
using Cognexa.Domain.Usuarios;
using Cognexa.Infrastructure.Inteligencia;
using Cognexa.Infrastructure.Persistencia;
using Cognexa.Tests.Compartilhado;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace Cognexa.Infrastructure.Tests;

public class ConteudoTests(PostgreSqlFixture banco) : IClassFixture<PostgreSqlFixture>
{
    private sealed class Tempo : TimeProvider
    {
        public DateTimeOffset Agora { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Agora;
    }
    [Fact]
    public void OcrExpiraIsolaUsuarioEConsomeUmaVez()
    {
        var tempo = new Tempo();
        using var store = new OcrTemporario(tempo);
        var usuario = Guid.NewGuid();
        var id = store.Guardar(usuario, "texto protegido");
        Assert.Throws<NaoEncontradoException>(() => store.Consumir(Guid.NewGuid(), id));
        Assert.Equal("texto protegido", store.Consumir(usuario, id));
        Assert.Throws<NaoEncontradoException>(() => store.Consumir(usuario, id));
        id = store.Guardar(usuario, "expirado");
        tempo.Agora = tempo.Agora.AddSeconds(120);
        Assert.Throws<NaoEncontradoException>(() => store.Consumir(usuario, id));
        for (var i = 0; i < 3; i++)
            store.Guardar(usuario, "texto");
        Assert.Throws<RegraDeDominioException>(() => store.Guardar(usuario, "excesso"));
    }
    private sealed class Handler : HttpMessageHandler
    {
        public string Payload { get; set; } = "{\"texto\":\"selecionável\"}";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Assert.Equal("https://ocr.example/extrair", request.RequestUri!.AbsoluteUri);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Payload) });
        }
    }
    [Fact]
    public async Task ProviderOcrValidaContratoESanitizaErros()
    {
        var handler = new Handler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://ocr.example/") };
        var provider = new OcrProvider(http);
        Assert.Equal("selecionável", await provider.ExtrairAsync([137], "image/png", default));
        handler.Payload = "conteúdo protegido inválido";
        var erro = await Assert.ThrowsAsync<ServicoIndisponivelException>(() => provider.ExtrairAsync([137], "image/png", default));
        Assert.DoesNotContain(handler.Payload, erro.ToString());
        handler.Payload = new string('x', 500001);
        await Assert.ThrowsAsync<ServicoIndisponivelException>(() => provider.ExtrairAsync([137], "image/png", default));
    }
    [PostgreSqlFact]
    public async Task QuotaAtomicaPersisteSemTextoERollbackNaoConsomeVolume()
    {
        var usuario = new Usuario("Teste");
        var livro = new Livro(usuario.Id, "Obra", ["Autor"]);
        await using (var setup = banco.Contexto())
        {
            setup.AddRange(usuario, livro);
            await setup.SalvarAsync();
        }
        var options = Options.Create(new ConteudoOptions { MaximoAcumulado = 150 });
        async Task<bool> Capturar()
        {
            await using var db = banco.Contexto();
            var controle = new ControleDeConteudo(db, options, TimeProvider.System);
            try
            {
                return await controle.ExecutarAsync(usuario.Id, livro.Id, new string('x', 100), 1, async () =>
                {
                    db.Add(new Anotacao(usuario.Id, livro.Id, null, TipoDeAnotacao.Citacao, new string('x', 100), null, 1, null, DateTimeOffset.UtcNow));
                    await db.SalvarAsync();
                    return true;
                }, default);
            }
            catch (RegraDeDominioException) { return false; }
        }
        var resultados = await Task.WhenAll(Capturar(), Capturar());
        Assert.Single(resultados, x => x);
        await using var verificar = banco.Contexto();
        var nota = await verificar.Set<Anotacao>().SingleAsync(x => x.IdLivro == livro.Id);
        verificar.Remove(nota);
        await verificar.SalvarAsync();
        Assert.False(await Capturar()); // exclusão não reinicia quota
        Assert.Single(await verificar.Set<RegistroDeConteudo>().Where(x => x.IdLivro == livro.Id).ToListAsync());
        await using var falha = banco.Contexto();
        var controleFalha = new ControleDeConteudo(falha, options, TimeProvider.System);
        await Assert.ThrowsAsync<InvalidOperationException>(() => controleFalha.ExecutarAsync<bool>(usuario.Id, livro.Id, "curto", null, () => throw new InvalidOperationException("falha"), default));
        Assert.Equal(100, await verificar.Set<RegistroDeConteudo>().Where(x => x.IdLivro == livro.Id).SumAsync(x => x.Caracteres));
    }
    [PostgreSqlFact]
    public async Task ProvenienciaPersisteESerializaEConcorrenciaProtegeModeracao()
    {
        await using var db = banco.Contexto();
        var usuario = new Usuario("Teste");
        var livro = new Livro(usuario.Id, "Obra", ["Autor"]);
        var a = new Anotacao(usuario.Id, livro.Id, null, TipoDeAnotacao.Citacao, "original", "comentário", 1, null, DateTimeOffset.UtcNow);
        a.DefinirProveniencia(MetodoDeCaptura.Ocr, OrigemDoConteudo.InteligenciaArtificial, "Obra", "Autor");
        a.Publicar(true);
        db.AddRange(usuario, livro, a);
        await db.SalvarAsync();
        db.ChangeTracker.Clear();
        var salva = await db.Set<Anotacao>().SingleAsync(x => x.Id == a.Id);
        Assert.Equal(MetodoDeCaptura.Ocr, salva.MetodoDeCaptura);
        var json = System.Text.Json.JsonSerializer.Serialize(salva);
        Assert.Contains("\"Obra\":\"Obra\"", json);
        Assert.Contains("\"Autor\":\"Autor\"", json);
        await using var concorrente = banco.Contexto();
        var antiga = await concorrente.Set<Anotacao>().SingleAsync(x => x.Id == a.Id);
        salva.Denunciar();
        db.Add(new Denuncia(salva.Id, usuario.Id, MotivoDeDenuncia.DireitosAutorais, DateTimeOffset.UtcNow));
        await db.SalvarAsync();
        antiga.Publicar(true);
        antiga.DefinirProveniencia(MetodoDeCaptura.Manual, OrigemDoConteudo.Manual, "Obra", "Autor");
        await Assert.ThrowsAsync<ConflitoException>(() => concorrente.SalvarAsync());
        salva.Retirar();
        await db.SalvarAsync();
        db.ChangeTracker.Clear();
        Assert.Equal(EstadoDePublicacao.Removido, (await db.Set<Anotacao>().SingleAsync(x => x.Id == a.Id)).Publicacao);
        Assert.Single(await db.Set<Denuncia>().Where(x => x.IdAnotacao == a.Id).ToListAsync());
    }
}

public class DescarteOcrTests
{
    private sealed class Usuario : IUsuarioAtual
    {
        public Guid IdUsuario { get; } = Guid.NewGuid();
    }
    private sealed class FalhaOcr : IOcrProvider
    {
        public Task<string> ExtrairAsync(byte[] imagem, string tipo, CancellationToken ct) => throw new ServicoIndisponivelException("Falha");
    }
    [Fact]
    public async Task FalhaDoProviderApagaBufferDaImagem()
    {
        using var store = new OcrTemporario(TimeProvider.System);
        var service = new OcrAppService(new FalhaOcr(), store, new Usuario(), null!);
        byte[] imagem = [137, 80, 78, 71, 13, 10, 26, 10];
        await Assert.ThrowsAsync<ServicoIndisponivelException>(() => service.ExtrairAsync(imagem, "image/png", default));
        Assert.All(imagem, x => Assert.Equal((byte)0, x));
        byte[] invalida = [1, 2];
        await Assert.ThrowsAsync<RegraDeDominioException>(() => service.ExtrairAsync(invalida, "image/png", default));
        Assert.All(invalida, x => Assert.Equal((byte)0, x));
    }
}
