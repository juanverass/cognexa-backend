using Cognexa.Application.Anotacoes;
using Cognexa.Application.Biblioteca;
using Cognexa.Application.Compartilhado;
using Cognexa.Application.Conhecimento;
using Cognexa.Domain.Compartilhado;
namespace Cognexa.Application.Inteligencia;

public enum OperacaoInteligente
{
    SugerirAprendizado, SintetizarAnotacoes, SugerirAplicacao, GerarPerguntas, DetectarIdeiasSemelhantes, SugerirRelacoes, ResumirLivro, ResumirCapitulo
}
public record FonteInteligenciaDto(Guid Id, string Tipo, string Conteudo, Guid? IdLivro = null);
public record PedidoInteligenciaDto(OperacaoInteligente Operacao, Guid[] IdAnotacoes, Guid[] IdAprendizados, Guid[] IdConceitos, Guid? IdLivro = null, Guid? IdCapitulo = null);
public record ConteudoGeradoDto(string Conteudo, Guid[] IdFontes);
public record SugestaoInteligenteDto(string Conteudo, IReadOnlyList<FonteInteligenciaDto> Fontes, DateTimeOffset DataDeGeracao, bool GeradoPorIA = true, bool RequerConfirmacao = true);
public record ResultadoSemanticoDto(Guid Id, string Tipo, string Conteudo, Guid? IdLivro, double Similaridade);
public interface IInteligenciaProvider
{
    Task<ConteudoGeradoDto> GerarAsync(OperacaoInteligente operacao, IReadOnlyList<FonteInteligenciaDto> fontes, CancellationToken ct);
}
public interface IBuscaSemanticaProvider
{
    Task<IReadOnlyList<ResultadoSemanticoDto>> BuscarAsync(Guid idUsuario, string consulta, int limite, CancellationToken ct);
}
public sealed class InteligenciaAppService(AnotacaoAppService anotacoes, AprendizadoAppService aprendizados, ConceitoAppService conceitos, LivroAppService biblioteca, IInteligenciaProvider provider, IBuscaSemanticaProvider busca, IUsuarioAtual atual, TimeProvider tempo)
{
    public async Task<SugestaoInteligenteDto> SugerirAsync(PedidoInteligenciaDto dto, CancellationToken ct)
    {
        if (!Enum.IsDefined(dto.Operacao) || dto.IdAnotacoes is null || dto.IdAprendizados is null || dto.IdConceitos is null
            || dto.IdAnotacoes.Length + dto.IdAprendizados.Length + dto.IdConceitos.Length > 100)
            throw new RegraDeDominioException("Operação ou seleção de fontes inválida (máximo 100).");
        var fontes = new List<FonteInteligenciaDto>();
        if (dto.Operacao is OperacaoInteligente.ResumirLivro or OperacaoInteligente.ResumirCapitulo)
        {
            if (!dto.IdLivro.HasValue || (dto.Operacao == OperacaoInteligente.ResumirCapitulo && !dto.IdCapitulo.HasValue))
                throw new RegraDeDominioException("Informe livro e capítulo quando aplicável.");
            await biblioteca.ExigirLivroAsync(dto.IdLivro.Value, ct);
            if (dto.IdCapitulo.HasValue)
                await biblioteca.ExigirCapituloAsync(dto.IdLivro.Value, dto.IdCapitulo.Value, ct);
            if (dto.IdAnotacoes.Length + dto.IdAprendizados.Length + dto.IdConceitos.Length > 0)
                throw new RegraDeDominioException("Resumo usa exclusivamente as anotações do livro ou capítulo informado.");
            var origens = await anotacoes.ListarAsync(new(IdLivro: dto.IdLivro, IdCapitulo: dto.IdCapitulo, Limite: 101), ct);
            if (origens.Count > 100)
                throw new RegraDeDominioException("Resumo excede 100 fontes; selecione um capítulo menor.");
            fontes.AddRange(origens.Select(Converter));
        }
        else
        {
            foreach (var id in dto.IdAnotacoes.Distinct())
                fontes.Add(Converter(await anotacoes.ObterAsync(id, ct)));
            foreach (var id in dto.IdAprendizados.Distinct())
            {
                var origem = await aprendizados.ObterAsync(id, ct);
                fontes.Add(new(origem.Id, "Aprendizado", origem.Conteudo));
            }
            foreach (var id in dto.IdConceitos.Distinct())
            {
                var origem = await conceitos.ObterAsync(id, ct);
                fontes.Add(new(origem.Id, "Conceito", origem.Nome));
            }
        }
        if (fontes.Count == 0)
            throw new RegraDeDominioException("Selecione pelo menos uma fonte armazenada.");
        var resultado = await provider.GerarAsync(dto.Operacao, fontes, ct);
        if (string.IsNullOrWhiteSpace(resultado.Conteudo) || resultado.Conteudo.Length > 20000 || resultado.IdFontes is null || resultado.IdFontes.Length == 0
            || resultado.IdFontes.Any(id => fontes.All(f => f.Id != id)))
            throw new ServicoIndisponivelException("Provider retornou conteúdo sem fontes válidas.");
        return new(resultado.Conteudo, fontes.Where(f => resultado.IdFontes.Contains(f.Id)).ToArray(), tempo.GetUtcNow());
    }
    private static FonteInteligenciaDto Converter(AnotacaoDto origem) => new(origem.Id, "Anotacao", $"Trecho original: {origem.TrechoOriginal}\nComentário do usuário: {origem.Comentario}", origem.IdLivro);
    public Task<IReadOnlyList<ResultadoSemanticoDto>> BuscarAsync(string consulta, int limite, CancellationToken ct)
    {
        consulta = Validacao.Texto(consulta, "Consulta", 2000);
        if (limite is < 1 or > 50)
            throw new RegraDeDominioException("Limite deve estar entre 1 e 50.");
        return busca.BuscarAsync(atual.IdUsuario, consulta, limite, ct);
    }
}
