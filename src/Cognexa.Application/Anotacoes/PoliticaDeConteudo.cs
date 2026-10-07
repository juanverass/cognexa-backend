using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Compartilhado;
namespace Cognexa.Application.Anotacoes;
public sealed class ConteudoOptions
{
    public int MaximoPorTrecho { get; set; } = 2000;
    public int MaximoAcumulado { get; set; } = 10000;
    public int JanelaHoras { get; set; } = 24;
    public int MaximoPaginasSequenciais { get; set; } = 5;
    public int MaximoPublicoPorTrecho { get; set; } = 500;
    public int MaximoPublicoPorLivro { get; set; } = 2000;
    public bool SocialHabilitado { get; set; }
    public string? RegistroRevisaoJuridica { get; set; }
    public bool GateAberto => SocialHabilitado && !string.IsNullOrWhiteSpace(RegistroRevisaoJuridica);
    public void Validar()
    {
        if (MaximoPorTrecho < 1 || MaximoAcumulado < 1 || JanelaHoras is < 1 or > 8760 || MaximoPaginasSequenciais < 3 || MaximoPublicoPorTrecho < 1 || MaximoPublicoPorLivro < 1)
            throw new InvalidOperationException("Política de conteúdo inválida.");
    }
}
public static class PoliticaDeConteudo
{
    public static void Validar(ConteudoOptions options, int caracteres, int? pagina, IReadOnlyList<RegistroDeConteudo> registros)
    {
        options.Validar();
        if (caracteres > options.MaximoPorTrecho || registros.Sum(x => (long)x.Caracteres) + caracteres > options.MaximoAcumulado)
            throw new RegraDeDominioException("Limite técnico de citações excedido.");
        if (pagina.HasValue)
        {
            var paginas = registros.Where(x => x.Pagina.HasValue).Select(x => x.Pagina!.Value).Append(pagina.Value).Distinct().Order().ToArray();
            var sequencia = 1;
            for (var i = 1; i < paginas.Length; i++)
            {
                sequencia = paginas[i] == paginas[i - 1] + 1 ? sequencia + 1 : 1;
                if (sequencia >= options.MaximoPaginasSequenciais) throw new RegraDeDominioException("Captura sequencial exige revisão.");
            }
        }
    }
}
public interface IControleDeConteudo
{
    Task ValidarPublicacaoAsync(Guid idUsuario, Guid idLivro, Guid idAnotacao, string? trecho, CancellationToken ct);
    Task<T> ExecutarAsync<T>(Guid idUsuario, Guid idLivro, string? trecho, int? pagina, Func<Task<T>> acao, CancellationToken ct);
}
