using System.Text.Json;
using System.Text.Json.Serialization;
using Cognexa.Application.Compartilhado;
using Cognexa.Application.Inteligencia;
using Microsoft.Extensions.Options;
namespace Cognexa.Infrastructure.Inteligencia;

public sealed class ProviderCompativel(HttpClient client, IOptions<InteligenciaOptions> options) : IInteligenciaProvider, IVetorizador
{
    private readonly TransporteDoProvider _transporte = new(client);
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public async Task<ConteudoGeradoDto> GerarAsync(OperacaoInteligente operacao, IReadOnlyList<FonteInteligenciaDto> fontes, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Modelo))
            throw new ServicoIndisponivelException("Modelo de geração não configurado.");
        var resposta = await _transporte.EnviarAsync<RespostaChat>("chat/completions", new
        {
            model = options.Value.Modelo,
            store = false,
            messages = new[]
            {
                new { role = "system", content = TransporteDoProvider.Instrucao(operacao) },
                new { role = "user", content = JsonSerializer.Serialize(fontes, Json) }
            },
            response_format = new
            {
                type = "json_object"
            }
        }, ct);
        var escolha = resposta.Choices?.FirstOrDefault();
        if (escolha?.FinishReason != "stop" || escolha.Message is null || !string.IsNullOrWhiteSpace(escolha.Message.Refusal) || string.IsNullOrWhiteSpace(escolha.Message.Content))
            throw new ServicoIndisponivelException("Provider não retornou uma geração completa.");
        try
        {
            return JsonSerializer.Deserialize<ConteudoGeradoDto>(escolha.Message.Content, Json) ?? throw new ServicoIndisponivelException("Geração vazia.");
        }
        catch (JsonException) { throw new ServicoIndisponivelException("Geração não corresponde ao contrato JSON."); }
    }
    public async Task<double[][]> VetorizarAsync(IReadOnlyList<string> textos, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ModeloDeVetores))
            throw new ServicoIndisponivelException("Modelo de vetores não configurado.");
        if (textos.Count == 0 || textos.Any(string.IsNullOrWhiteSpace))
            throw new ServicoIndisponivelException("Textos de vetorização inválidos.");
        // Dividir textos longos evita exceder o limite por entrada e por lote do provider.
        var segmentos = new List<(int IndiceTexto, string Texto)>();
        for (var i = 0; i < textos.Count; i++)
        {
            for (var posicao = 0; posicao < textos[i].Length;)
            {
                var tamanho = Math.Min(2000, textos[i].Length - posicao);
                if (char.IsHighSurrogate(textos[i][posicao + tamanho - 1]) && posicao + tamanho < textos[i].Length)
                    tamanho--;
                segmentos.Add((i, textos[i].Substring(posicao, tamanho)));
                posicao += tamanho;
            }
        }
        var somas = new double[textos.Count][];
        var pesos = new int[textos.Count];
        foreach (var lote in segmentos.Chunk(32))
        {
            var resposta = await _transporte.EnviarAsync<RespostaEmbeddings>("embeddings", new
            {
                model = options.Value.ModeloDeVetores,
                input = lote.Select(x => x.Texto).ToArray(),
                encoding_format = "float"
            }, ct);
            if (resposta.Data is null || resposta.Data.Length != lote.Length || !resposta.Data.Select(x => x.Index).Order().SequenceEqual(Enumerable.Range(0, lote.Length)))
                throw new ServicoIndisponivelException("Índices de vetores inválidos.");
            var vetores = resposta.Data.OrderBy(x => x.Index).Select(x => x.Embedding).ToArray();
            TransporteDoProvider.ValidarVetores(vetores, lote.Length);
            for (var i = 0; i < lote.Length; i++)
            {
                var indice = lote[i].IndiceTexto;
                var vetor = vetores[i];
                var peso = lote[i].Texto.Length;
                somas[indice] ??= new double[vetor.Length];
                if (somas[indice].Length != vetor.Length)
                    throw new ServicoIndisponivelException("Dimensão mudou entre os lotes.");
                var escala = vetor.Max(x => Math.Abs(x));
                var norma = Math.Sqrt(vetor.Sum(x => Math.Pow(x / escala, 2)));
                for (var d = 0; d < vetor.Length; d++)
                    somas[indice][d] += (vetor[d] / escala / norma) * peso;
                pesos[indice] += peso;
            }
        }
        for (var i = 0; i < somas.Length; i++)
            for (var d = 0; d < somas[i].Length; d++)
                somas[i][d] /= pesos[i];
        TransporteDoProvider.ValidarVetores(somas, textos.Count);
        return somas;
    }
    private sealed record RespostaChat(Escolha[]? Choices);
    private sealed record Escolha(Mensagem? Message, [property: JsonPropertyName("finish_reason")] string? FinishReason);
    private sealed record Mensagem(string? Content, string? Refusal);
    private sealed record RespostaEmbeddings(Vetor[]? Data);
    private sealed record Vetor(int Index, double[] Embedding);
}
