using System.Net;
using System.Text;
using Cognexa.Application.Compartilhado;
using Cognexa.Application.Inteligencia;
using Cognexa.Domain.Revisoes;
using Cognexa.Infrastructure.Inteligencia;
using Cognexa.Infrastructure.Revisoes;
namespace Cognexa.Infrastructure.Tests;

public class ProvidersTests
{
    [Fact]
    public void SimilaridadeValidaDimensoesENormalizaVetores()
    {
        Assert.Equal(1, BuscaSemanticaProvider.Similaridade([2, 0], [1, 0]));
        Assert.Equal(0, BuscaSemanticaProvider.Similaridade([1, 0], [0, 1]));
        Assert.Equal(-1, BuscaSemanticaProvider.Similaridade([1, 0], [-1, 0]));
        Assert.Equal(1, BuscaSemanticaProvider.Similaridade([double.MaxValue, 0], [1, 0]));
        Assert.Throws<ServicoIndisponivelException>(() => BuscaSemanticaProvider.Similaridade([0, 0], [1, 0]));
        Assert.Throws<ServicoIndisponivelException>(() => BuscaSemanticaProvider.Similaridade([1], [1, 0]));
    }
    [Fact]
    public void AgendadorProgrideEReagendaErro()
    {
        var agora = DateTimeOffset.UtcNow;
        var revisao = new Revisao(Guid.NewGuid(), Guid.NewGuid(), agora);
        var agendador = new AgendadorDeRevisao();
        var primeira = agendador.CalcularProxima(revisao, ResultadoDaRevisao.Bom, agora);
        revisao.Registrar(ResultadoDaRevisao.Bom, null, agora, primeira);
        var segunda = agendador.CalcularProxima(revisao, ResultadoDaRevisao.Bom, primeira);
        Assert.True(segunda - primeira > primeira - agora);
        Assert.Equal(primeira.AddDays(1), agendador.CalcularProxima(revisao, ResultadoDaRevisao.Errou, primeira));
    }
    [Theory]
    [InlineData("invalid json", 200)]
    [InlineData("{}", 502)]
    public async Task ProviderInvalidoOuIndisponivelProduzErroSeguro(string payload, int status)
    {
        using var client = new HttpClient(new RespostaHandler(payload, status)) { BaseAddress = new("https://provider.example/") };
        await Assert.ThrowsAsync<ServicoIndisponivelException>(() => new ProviderHttp(client).GerarAsync(OperacaoInteligente.SugerirAprendizado, [], default));
    }
    private sealed class RespostaHandler(string payload, int status) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => Task.FromResult(new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent(payload, Encoding.UTF8, "application/json") });
    }
}
