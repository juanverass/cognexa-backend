using Cognexa.Application.Inteligencia;
namespace Cognexa.Infrastructure.Inteligencia;
// Protocolo de gateway que permite providers com contratos próprios.
public sealed class ProviderHttp(HttpClient client) : IInteligenciaProvider, IVetorizador
{
    private readonly TransporteDoProvider _transporte = new(client);
    public Task<ConteudoGeradoDto> GerarAsync(OperacaoInteligente operacao, IReadOnlyList<FonteInteligenciaDto> fontes, CancellationToken ct) =>
        _transporte.EnviarAsync<ConteudoGeradoDto>("geracoes", new
        {
            Operacao = operacao.ToString(),
            Fontes = fontes,
            Instrucao = TransporteDoProvider.Instrucao(operacao)
        }, ct);
    public async Task<double[][]> VetorizarAsync(IReadOnlyList<string> textos, CancellationToken ct)
    {
        var resposta = await _transporte.EnviarAsync<RespostaVetores>("vetores", new
        {
            Textos = textos
        }, ct);
        TransporteDoProvider.ValidarVetores(resposta.Vetores, textos.Count);
        return resposta.Vetores;
    }
    private sealed record RespostaVetores(double[][] Vetores);
}
