using Cognexa.Application.Anotacoes;
using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Compartilhado;
namespace Cognexa.Application.Tests;

public sealed class ControleTeste : IControleDeConteudo
{
    private readonly List<RegistroDeConteudo> registros = [];
    public async Task<T> ExecutarAsync<T>(Guid usuario, Guid livro, string? trecho, int? pagina, Func<Task<T>> acao, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(trecho))
        {
            PoliticaDeConteudo.Validar(new(), trecho.Length, pagina, registros.Where(x => x.IdLivro == livro).ToArray());
            registros.Add(new(usuario, livro, trecho.Length, pagina, DateTimeOffset.UtcNow));
        }
        return await acao();
    }
}
public class ConteudoTests
{
    [Fact]
    public void AcumulacaoESequenciaBloqueiamReconstrucao()
    {
        var livro = Guid.NewGuid();
        var usuario = Guid.NewGuid();
        var registros = Enumerable.Range(1, 4).Select(p => new RegistroDeConteudo(usuario, livro, 100, p, DateTimeOffset.UtcNow)).ToArray();
        Assert.Throws<RegraDeDominioException>(() => PoliticaDeConteudo.Validar(new(), 100, 5, registros));
        Assert.Throws<RegraDeDominioException>(() => PoliticaDeConteudo.Validar(new() { MaximoAcumulado = 450 }, 100, 20, registros));
        PoliticaDeConteudo.Validar(new(), 100, 4, registros);
        Assert.Throws<RegraDeDominioException>(() => PoliticaDeConteudo.Validar(new(), 2001, null, []));
    }
    [Fact]
    public void GateExigeRegistroEOptIn()
    {
        Assert.False(new ConteudoOptions { SocialHabilitado = true }.GateAberto);
        Assert.False(new ConteudoOptions { RegistroRevisaoJuridica = "parecer" }.GateAberto);
    }
}
