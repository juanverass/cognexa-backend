using System.Net;
using System.Text;
using System.Text.Json;
using Cognexa.Application.Compartilhado;
using Cognexa.Application.Inteligencia;
using Cognexa.Infrastructure.Inteligencia;
using Microsoft.Extensions.Options;
namespace Cognexa.Infrastructure.Tests;

public class ProviderCompativelTests
{
    private static ProviderCompativel Criar(HttpMessageHandler handler) => new(new HttpClient(handler) { BaseAddress = new("https://provider.example/v1/") }, Options.Create(new InteligenciaOptions { Modelo = "modelo-teste", ModeloDeVetores = "vetores-teste" }));
    [Fact]
    public async Task GeracaoUsaJsonEConservaIdsDeFontes()
    {
        var id = Guid.NewGuid();
        var conteudo = JsonSerializer.Serialize(new
        {
            conteudo = "Síntese",
            idFontes = new[] { id }
        });
        var payload = JsonSerializer.Serialize(new
        {
            choices = new[] { new { finish_reason = "stop", message = new { content = conteudo } } }
        });
        var handler = new RespostaHandler(payload);
        var resultado = await Criar(handler).GerarAsync(OperacaoInteligente.SugerirAprendizado, [new(id, "Anotacao", "Original")], default);
        Assert.Equal(id, resultado.IdFontes.Single());
        Assert.Equal("Síntese", resultado.Conteudo);
        Assert.EndsWith("chat/completions", handler.Rota);
        Assert.Contains("json_object", handler.Enviado);
        Assert.Contains(id.ToString(), handler.Enviado);
        using var json = JsonDocument.Parse(handler.Enviado);
        Assert.False(json.RootElement.GetProperty("store").GetBoolean());
    }
    [Theory]
    [InlineData("length", null)]
    [InlineData("stop", "recusado")]
    public async Task RecusaOuTruncamentoNaoProduzemSugestao(string finishReason, string? refusal)
    {
        var payload = JsonSerializer.Serialize(new
        {
            choices = new[] { new { finish_reason = finishReason, message = new { content = "{}", refusal } } }
        });
        await Assert.ThrowsAsync<ServicoIndisponivelException>(() => Criar(new RespostaHandler(payload)).GerarAsync(OperacaoInteligente.GerarPerguntas, [], default));
    }
    [Fact]
    public async Task VetoresRespeitamIndicesETextosLongosSaoDivididos()
    {
        var handler = new EmbeddingsHandler();
        var vetores = await Criar(handler).VetorizarAsync(["Consulta", new string('x', 70000)], default);
        Assert.Equal(2, vetores.Length);
        Assert.Equal(1, vetores[0][0]);
        Assert.Equal(1, vetores[1][0]);
        Assert.True(handler.Requisicoes >= 2);
        Assert.InRange(handler.MaiorTexto, 1, 2000);
    }
    [Fact]
    public async Task VetoresComIndicesDuplicadosSaoRejeitados()
    {
        const string payload = "{\"data\":[{\"index\":0,\"embedding\":[1,0]},{\"index\":0,\"embedding\":[1,0]}]}";
        await Assert.ThrowsAsync<ServicoIndisponivelException>(() => Criar(new RespostaHandler(payload)).VetorizarAsync(["a", "b"], default));
    }
    [Fact]
    public async Task SemConfiguracaoNaoEnviaChamadasExternas()
    {
        var handler = new RespostaHandler("{}");
        var provider = new ProviderCompativel(new HttpClient(handler), Options.Create(new InteligenciaOptions()));
        await Assert.ThrowsAsync<ServicoIndisponivelException>(() => provider.GerarAsync(OperacaoInteligente.SugerirAprendizado, [], default));
        Assert.Equal("", handler.Rota);
    }
    private sealed class RespostaHandler(string payload) : HttpMessageHandler
    {
        public string Enviado { get; private set; } = "";
        public string Rota { get; private set; } = "";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Rota = request.RequestUri!.AbsolutePath;
            Enviado = await request.Content!.ReadAsStringAsync(cancellationToken);
            return new(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        }
    }
    private sealed class EmbeddingsHandler : HttpMessageHandler
    {
        public int Requisicoes
        {
            get; private set;
        }
        public int MaiorTexto
        {
            get; private set;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requisicoes++;
            using var json = JsonDocument.Parse(await request.Content!.ReadAsStringAsync(cancellationToken));
            var textos = json.RootElement.GetProperty("input").EnumerateArray().Select(x => x.GetString()!).ToArray();
            MaiorTexto = Math.Max(MaiorTexto, textos.Max(x => x.Length));
            var payload = JsonSerializer.Serialize(new
            {
                data = Enumerable.Range(0, textos.Length).Reverse().Select(i => new { index = i, embedding = new[] { 1d, 0d } }).ToArray()
            });
            return new(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };
        }
    }
}
