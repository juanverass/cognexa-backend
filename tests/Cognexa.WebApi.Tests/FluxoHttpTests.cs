using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cognexa.Application.Anotacoes;
using Cognexa.Application.Biblioteca;
using Cognexa.Application.Conhecimento;
using Cognexa.Application.Revisoes;
using Cognexa.Application.Usuarios;
using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Biblioteca;
using Cognexa.Domain.Compartilhado;
using Cognexa.Domain.Revisoes;
using Cognexa.Tests.Compartilhado;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Cognexa.WebApi.Tests;

public class FluxoHttpTests(PostgreSqlFixture banco) : IClassFixture<PostgreSqlFixture>
{
    private const string TokenA = "token-de-teste-usuario-a-1234567890";
    private const string TokenB = "token-de-teste-usuario-b-1234567890";
    private sealed class TempoControlado : TimeProvider
    {
        public DateTimeOffset Agora { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Agora;
    }
    private WebApplicationFactory<Program> Factory(TempoControlado tempo, bool social = false, int maximoPublico = 2000) => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.UseEnvironment("Production");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Cognexa"] = banco.ConnectionString,
            ["Conteudo:SocialHabilitado"] = social.ToString(),
            ["Conteudo:MaximoPublicoPorLivro"] = maximoPublico.ToString(),
            ["Conteudo:RegistroRevisaoJuridica"] = social ? "parecer-simulado-somente-teste" : null,
            ["Autenticacao:Identidades:0:IdIdentidade"] = Guid.NewGuid().ToString(),
            ["Autenticacao:Identidades:0:Token"] = TokenA,
            ["Autenticacao:Identidades:1:IdIdentidade"] = Guid.NewGuid().ToString(),
            ["Autenticacao:Identidades:1:Token"] = TokenB
        }));
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<TimeProvider>(tempo);
            services.AddSingleton<IOcrProvider, OcrTeste>();
        });
    });
    private static async Task<T> Ler<T>(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, $"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }
    [PostgreSqlFact]
    public async Task FluxoCompletoIsolaUsuariosPreservaFontesEHistorico()
    {
        var tempo = new TempoControlado();
        await using var factory = Factory(tempo);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenA);
        var usuario = await Ler<UsuarioDto>(await client.PostAsJsonAsync("/usuarios/me", new SalvarUsuarioDto("João")));
        Assert.NotEqual(Guid.Parse(factory.Services.GetRequiredService<IConfiguration>()["Autenticacao:Identidades:0:IdIdentidade"]!), usuario.Id);
        var preferencias = await Ler<PreferenciasDoUsuarioDto>(await client.PutAsJsonAsync("/usuarios/me/preferencias", new PreferenciasDoUsuarioDto(30, "pt-BR")));
        Assert.Equal(30, preferencias.MetaDiariaEmMinutos);
        var livro = await Ler<LivroDto>(await client.PostAsJsonAsync("/biblioteca/livros", new SalvarLivroDto("Livro", ["Autor"], 100)));
        var leitura = await Ler<LeituraDto>(await client.GetAsync($"/biblioteca/livros/{livro.Id}/leitura"));
        Assert.Equal(StatusDaLeitura.QueroLer, leitura.Status);
        Assert.Null(leitura.DataDeInicio);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync($"/biblioteca/livros/{livro.Id}/leitura", new SalvarLeituraDto(StatusDaLeitura.Concluido, 99))).StatusCode);
        await Ler<LeituraDto>(await client.PutAsJsonAsync($"/biblioteca/livros/{livro.Id}/leitura", new SalvarLeituraDto(StatusDaLeitura.Lendo, 10)));
        var capitulo = await Ler<CapituloDto>(await client.PostAsJsonAsync($"/biblioteca/livros/{livro.Id}/capitulos", new CriarCapituloDto("Primeiro", 1)));
        var nota = await Ler<AnotacaoDto>(await client.PostAsJsonAsync("/anotacoes", new SalvarAnotacaoDto(livro.Id, capitulo.Id, TipoDeAnotacao.Insight, "  Original  ", "Comentário", 10)));
        var notas = await Ler<AnotacaoDto[]>(await client.GetAsync($"/anotacoes?idLivro={livro.Id}&tipo=Insight&pagina=10"));
        Assert.Equal(nota.Id, notas.Single().Id);
        var conceito = await Ler<ConceitoDto>(await client.PostAsJsonAsync("/conhecimento/conceitos", new SalvarConceitoDto("Tema")));
        var aprendizado = await Ler<AprendizadoDto>(await client.PostAsJsonAsync("/conhecimento/aprendizados", new CriarAprendizadoDto("Síntese", [nota.Id], [conceito.Id], OrigemDoConteudo.InteligenciaArtificial)));
        Assert.Equal(OrigemDoConteudo.InteligenciaArtificial, aprendizado.Origem);
        Assert.Equal(nota.Id, aprendizado.Fontes.Single().IdAnotacao);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/anotacoes/{nota.Id}")).StatusCode);
        var fonte = await Ler<AnotacaoDto>(await client.GetAsync($"/anotacoes/{nota.Id}"));
        Assert.Equal("  Original  ", fonte.TrechoOriginal);
        Assert.Equal(HttpStatusCode.Conflict, (await client.DeleteAsync($"/biblioteca/livros/{livro.Id}")).StatusCode);
        var pergunta = await Ler<PerguntaAgendadaDto>(await client.PostAsJsonAsync("/revisoes/perguntas", new CriarPerguntaDto(aprendizado.Id, "Pergunta?", "Resposta")));
        var devidas = await Ler<RevisaoDto[]>(await client.GetAsync($"/revisoes/devidas?idLivro={livro.Id}&idConceito={conceito.Id}"));
        Assert.Equal(pergunta.Revisao.Id, devidas.Single().Id);
        var revisao = await Ler<RevisaoDto>(await client.PostAsJsonAsync($"/revisoes/{pergunta.Revisao.Id}/respostas", new RegistrarRevisaoDto(ResultadoDaRevisao.Bom, "Resposta")));
        Assert.Equal(NivelDeDominio.EmAprendizado, revisao.NivelDeDominio);
        tempo.Agora = revisao.ProximaRevisao;
        await Ler<RevisaoDto>(await client.PostAsJsonAsync($"/revisoes/{pergunta.Revisao.Id}/respostas", new RegistrarRevisaoDto(ResultadoDaRevisao.Errou, "Não sei")));
        Assert.Equal(2, (await Ler<HistoricoDeRevisaoDto[]>(await client.GetAsync($"/revisoes/{pergunta.Revisao.Id}/historico"))).Length);
        client.DefaultRequestHeaders.Authorization = new("Bearer", TokenB);
        await Ler<UsuarioDto>(await client.PostAsJsonAsync("/usuarios/me", new SalvarUsuarioDto("Outro")));
        foreach (var path in new[] { $"/biblioteca/livros/{livro.Id}", $"/anotacoes/{nota.Id}", $"/conhecimento/aprendizados/{aprendizado.Id}", $"/revisoes/{pergunta.Revisao.Id}/historico" })
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync(path)).StatusCode);
        Assert.Empty(await Ler<AnotacaoDto[]>(await client.GetAsync("/anotacoes")));
        Assert.Equal(HttpStatusCode.NotFound, (await client.DeleteAsync($"/anotacoes/{nota.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/conhecimento/aprendizados", new CriarAprendizadoDto("Roubar fonte", [nota.Id], []))).StatusCode);
        client.DefaultRequestHeaders.Authorization = new("Bearer", TokenA);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/usuarios/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.GetAsync("/anotacoes")).StatusCode);
        Assert.NotEqual(Guid.Empty, usuario.Id);
    }
    [PostgreSqlFact]
    public async Task AutenticacaoErrosDeEntradaESaudeRetornamContratosSeguros()
    {
        await using var factory = Factory(new TempoControlado());
        using var client = factory.CreateClient();
        var semToken = await client.GetAsync("/anotacoes");
        Assert.Equal(HttpStatusCode.Unauthorized, semToken.StatusCode);
        Assert.Equal("application/problem+json", semToken.Content.Headers.ContentType!.MediaType);
        client.DefaultRequestHeaders.Authorization = new("Bearer", new string('x', 32));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/usuarios/me")).StatusCode);
        client.DefaultRequestHeaders.Authorization = new("Bearer", TokenA);
        var invalido = await client.PostAsJsonAsync("/usuarios/me", new SalvarUsuarioDto("  "));
        Assert.Equal(HttpStatusCode.BadRequest, invalido.StatusCode);
        var problem = await invalido.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.True(problem!.Extensions.ContainsKey("traceId"));
        var malformed = await client.PostAsync("/usuarios/me", new StringContent("{", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);
        Assert.Equal("application/problem+json", malformed.Content.Headers.ContentType!.MediaType);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/ready")).StatusCode);
    }

    private sealed class OcrTeste : IOcrProvider
    {
        public Task<string> ExtrairAsync(byte[] imagem, string tipo, CancellationToken ct) => Task.FromResult("página completa com trecho selecionado e restante");
    }
    [PostgreSqlFact]
    public async Task OcrPersisteSomenteSelecaoEGateBloqueiaPublicacao()
    {
        await using var factory = Factory(new()); using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", TokenA);
        await Ler<UsuarioDto>(await client.PostAsJsonAsync("/usuarios/me", new SalvarUsuarioDto("OCR")));
        var livro = await Ler<LivroDto>(await client.PostAsJsonAsync("/biblioteca/livros", new SalvarLivroDto("Obra", ["Autor"], 100)));
        using var imagem = new ByteArrayContent([137, 80, 78, 71, 13, 10, 26, 10]); imagem.Headers.ContentType = new("image/png");
        var captura = await Ler<ResultadoOcrDto>(await client.PostAsync("/anotacoes/ocr", imagem));
        var confirmacao = new ConfirmarOcrDto(captura.IdCaptura, 20, 17, livro.Id, 1, null, "interpretação");
        var nota = await Ler<AnotacaoDto>(await client.PostAsJsonAsync("/anotacoes/ocr/confirmar", confirmacao));
        Assert.Equal(captura.Texto.Substring(20, 17), nota.TrechoOriginal); Assert.Equal(MetodoDeCaptura.Ocr, nota.MetodoDeCaptura);
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsJsonAsync("/anotacoes/ocr/confirmar", confirmacao)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync($"/anotacoes/{nota.Id}/preparar-publicacao", null)).StatusCode);
        nota = await Ler<AnotacaoDto>(await client.PutAsJsonAsync($"/anotacoes/{nota.Id}", new SalvarAnotacaoDto(livro.Id, null, nota.Tipo, nota.TrechoOriginal, nota.Comentario, 1, null, MetodoDeCaptura.Ocr, OrigemDoConteudo.Manual, "Obra", "Autor")));
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"/anotacoes/{nota.Id}/preparar-publicacao", null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync($"/anotacoes/{nota.Id}/publicar", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/publicacoes/{nota.Id}")).StatusCode);
        await using var db = banco.Contexto();
        var salva = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleAsync(db.Set<Anotacao>(), x => x.Id == nota.Id);
        Assert.Equal(nota.TrechoOriginal, salva.TrechoOriginal);
        Assert.DoesNotContain("restante", salva.TrechoOriginal!);
    }
    [PostgreSqlFact]
    public async Task DenunciaOcultaERetiradaImpedeRepublicacao()
    {
        await using var factory = Factory(new(), true); using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", TokenA);
        await Ler<UsuarioDto>(await client.PostAsJsonAsync("/usuarios/me", new SalvarUsuarioDto("Dono")));
        var livro = await Ler<LivroDto>(await client.PostAsJsonAsync("/biblioteca/livros", new SalvarLivroDto("Obra", ["Autor"], 100)));
        var dto = new SalvarAnotacaoDto(livro.Id, null, TipoDeAnotacao.Citacao, "citação", "interpretação", 1, null, MetodoDeCaptura.Manual, OrigemDoConteudo.InteligenciaArtificial, "Obra", "Autor");
        var nota = await Ler<AnotacaoDto>(await client.PostAsJsonAsync("/anotacoes", dto));
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"/anotacoes/{nota.Id}/publicar", null)).StatusCode);
        await using (var fechada = Factory(new()))
        {
            using var semGate = fechada.CreateClient();
            Assert.Equal(HttpStatusCode.NotFound, (await semGate.GetAsync($"/publicacoes/{nota.Id}")).StatusCode);
        }
        using var publico = factory.CreateClient();
        var citacao = await Ler<CitacaoPublicaDto>(await publico.GetAsync($"/publicacoes/{nota.Id}")); Assert.Equal(OrigemDoConteudo.InteligenciaArtificial, citacao.OrigemDoComentario);
        client.DefaultRequestHeaders.Authorization = new("Bearer", TokenB);
        await Ler<UsuarioDto>(await client.PostAsJsonAsync("/usuarios/me", new SalvarUsuarioDto("Denunciante")));
        Assert.Equal(HttpStatusCode.NotFound, (await client.PostAsync($"/anotacoes/{nota.Id}/retirar", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync($"/publicacoes/{nota.Id}/denuncias", new Cognexa.WebApi.DenunciarDto(MotivoDeDenuncia.DireitosAutorais))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await publico.GetAsync($"/publicacoes/{nota.Id}")).StatusCode);
        client.DefaultRequestHeaders.Authorization = new("Bearer", TokenA);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"/anotacoes/{nota.Id}/retirar", null)).StatusCode);
        await Ler<AnotacaoDto>(await client.PutAsJsonAsync($"/anotacoes/{nota.Id}", dto with { TrechoOriginal = "edição" }));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync($"/anotacoes/{nota.Id}/publicar", null)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await publico.GetAsync($"/publicacoes/{nota.Id}")).StatusCode);
        Assert.Equal(EstadoDePublicacao.Removido, (await Ler<AnotacaoDto>(await client.GetAsync($"/anotacoes/{nota.Id}"))).Publicacao);
        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync($"/anotacoes/{nota.Id}")).StatusCode);
        await using var auditoria = banco.Contexto();
        var registros = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(auditoria.Set<RegistroDePublicacao>().Where(x => x.IdAnotacao == nota.Id));
        Assert.Equal(3, registros.Count); Assert.All(registros, x => Assert.Equal("Autor", x.Autor));
        Assert.DoesNotContain("citação", System.Text.Json.JsonSerializer.Serialize(registros));
    }

    [PostgreSqlFact]
    public async Task EdicoesERecadastrosCompartilhamLimitePublicoPorObra()
    {
        await using var factory = Factory(new(), true, 150); using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", TokenA);
        await Ler<UsuarioDto>(await client.PostAsJsonAsync("/usuarios/me", new SalvarUsuarioDto("Obras")));
        var original = await Ler<LivroDto>(await client.PostAsJsonAsync("/biblioteca/livros", new SalvarLivroDto("Ação e reflexão", ["João"], 100, "9780000000001", "1")));
        var edicao = await Ler<LivroDto>(await client.PostAsJsonAsync("/biblioteca/livros", new SalvarLivroDto(" ACAO E REFLEXAO! ", ["JOAO"], 200, "9780000000002", "2")));
        var semIsbn = await Ler<LivroDto>(await client.PostAsJsonAsync("/biblioteca/livros", new SalvarLivroDto("Ação e reflexão", ["João"])));
        async Task<AnotacaoDto> Anotar(Guid livro) => await Ler<AnotacaoDto>(await client.PostAsJsonAsync("/anotacoes", new SalvarAnotacaoDto(livro, null, TipoDeAnotacao.Citacao, new string('x', 100), "comentário", 1, null, MetodoDeCaptura.Manual, OrigemDoConteudo.Manual, "Ação e reflexão", "João")));
        var a = await Anotar(original.Id); var b = await Anotar(edicao.Id); var c = await Anotar(semIsbn.Id);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"/anotacoes/{a.Id}/publicar", null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync($"/anotacoes/{b.Id}/publicar", null)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync($"/anotacoes/{c.Id}/preparar-publicacao", null)).StatusCode);
        await Ler<LivroDto>(await client.PutAsJsonAsync($"/biblioteca/livros/{original.Id}", new SalvarLivroDto("Título editado", ["Autor editado"], 100)));
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync($"/anotacoes/{b.Id}/publicar", null)).StatusCode);
    }
}
