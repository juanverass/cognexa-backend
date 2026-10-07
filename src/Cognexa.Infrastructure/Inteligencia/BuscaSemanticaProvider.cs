using Cognexa.Application.Compartilhado;
using Cognexa.Application.Inteligencia;
using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Conhecimento;
using Cognexa.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
namespace Cognexa.Infrastructure.Inteligencia;
// Embeddings derivados são reconstruídos por lote; PostgreSQL permanece a fonte canônica.
public sealed class BuscaSemanticaProvider(CognexaDbContext contexto, IVetorizador provider) : IBuscaSemanticaProvider
{
    public async Task<IReadOnlyList<ResultadoSemanticoDto>> BuscarAsync(Guid idUsuario, string consulta, int limite, CancellationToken ct)
    {
        var anotacoes = contexto.Set<Anotacao>().AsNoTracking().Where(x => x.IdUsuario == idUsuario).OrderBy(x => x.Id)
            .Select(x => new FonteInteligenciaDto(x.Id, "Anotacao", (x.TrechoOriginal ?? "") + "\n" + (x.Comentario ?? ""), x.IdLivro));
        var aprendizados = contexto.Set<Aprendizado>().AsNoTracking().Where(x => x.IdUsuario == idUsuario).OrderBy(x => x.Id)
            .Select(x => new FonteInteligenciaDto(x.Id, "Aprendizado", x.Conteudo, null));
        var resultados = new List<ResultadoSemanticoDto>();
        foreach (var query in new[] { anotacoes, aprendizados })
        {
            var pagina = 0;
            while (true)
            {
                var lote = await query.Skip(pagina * 100).Take(100).ToArrayAsync(ct);
                if (lote.Length == 0)
                    break;
                var vetores = await provider.VetorizarAsync(new[] { consulta }.Concat(lote.Select(x => x.Conteudo)).ToArray(), ct);
                resultados.AddRange(lote.Select((f, i) => new ResultadoSemanticoDto(f.Id, f.Tipo, f.Conteudo, f.IdLivro, Similaridade(vetores[0], vetores[i + 1]))));
                resultados = resultados.OrderByDescending(x => x.Similaridade).ThenBy(x => x.Id).Take(limite).ToList();
                if (lote.Length < 100)
                    break;
                pagina++;
            }
        }
        return resultados;
    }
    public static double Similaridade(double[] a, double[] b)
    {
        if (a.Length == 0 || a.Length != b.Length || a.Any(x => !double.IsFinite(x)) || b.Any(x => !double.IsFinite(x)))
            throw new ServicoIndisponivelException("Vetores incompatíveis.");
        // Normalização por escala evita overflow com valores finitos grandes.
        var escalaA = a.Max(x => Math.Abs(x));
        var escalaB = b.Max(x => Math.Abs(x));
        if (escalaA == 0 || escalaB == 0)
            throw new ServicoIndisponivelException("Vetor nulo.");
        var produto = 0d;
        var normaA = 0d;
        var normaB = 0d;
        for (var i = 0; i < a.Length; i++)
        {
            var x = a[i] / escalaA;
            var y = b[i] / escalaB;
            produto += x * y;
            normaA += x * x;
            normaB += y * y;
        }
        return Math.Clamp(produto / Math.Sqrt(normaA * normaB), -1, 1);
    }
}
