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
    private WebApplicationFactory<Program> Factory(TempoControlado tempo) => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.UseEnvironment("Production");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Cognexa"] = banco.ConnectionString,
            ["Autenticacao:Identidades:0:IdIdentidade"] = Guid.NewGuid().ToString(),
            ["Autenticacao:Identidades:0:Token"] = TokenA,
            ["Autenticacao:Identidades:1:IdIdentidade"] = Guid.NewGuid().ToString(),
            ["Autenticacao:Identidades:1:Token"] = TokenB
        }));
        builder.ConfigureServices(services => services.AddSingleton<TimeProvider>(tempo));
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
}
