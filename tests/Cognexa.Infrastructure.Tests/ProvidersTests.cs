using System.Net;
using System.Text;
using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Revisoes;
using Cognexa.Infrastructure.Revisoes;
namespace Cognexa.Infrastructure.Tests;

public class ProvidersTests
{
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
}
