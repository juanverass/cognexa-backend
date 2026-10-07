using Cognexa.Application.Compartilhado;
using Cognexa.Application.Conhecimento;
using Cognexa.Domain.Compartilhado;
using Cognexa.Domain.Revisoes;
using Mapster;
namespace Cognexa.Application.Revisoes;

public interface IAgendadorDeRevisao
{
    DateTimeOffset CalcularProxima(Revisao revisao, ResultadoDaRevisao resultado, DateTimeOffset agora);
}
public interface IRevisaoRepository : IRepository<Revisao>
{
    Task<IReadOnlyList<Revisao>> ListarDevidasAsync(Guid idUsuario, DateTimeOffset agora, Guid? idLivro, Guid? idConceito, int limite, CancellationToken ct);
    Task<IReadOnlyList<HistoricoDeRevisao>> ListarHistoricoAsync(Guid idUsuario, Guid idRevisao, int pagina, CancellationToken ct);
}
public record CriarPerguntaDto(Guid IdAprendizado, string Pergunta, string RespostaEsperada, OrigemDoConteudo Origem = OrigemDoConteudo.Manual);
public record PerguntaDeRevisaoDto(Guid Id, Guid IdAprendizado, string Pergunta, string RespostaEsperada, OrigemDoConteudo Origem);
public record RevisaoDto(Guid Id, Guid IdPergunta, DateTimeOffset ProximaRevisao, DateTimeOffset? UltimaRevisao, int AcertosConsecutivos, NivelDeDominio NivelDeDominio);
public record RegistrarRevisaoDto(ResultadoDaRevisao Resultado, string? Resposta);
public record HistoricoDeRevisaoDto(Guid Id, ResultadoDaRevisao Resultado, string? Resposta, DateTimeOffset Data, DateTimeOffset ProximaRevisao, NivelDeDominio NivelAnterior, NivelDeDominio NivelNovo);
public record PerguntaAgendadaDto(PerguntaDeRevisaoDto Pergunta, RevisaoDto Revisao);
public sealed class MapeamentoRevisao : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PerguntaDeRevisao, PerguntaDeRevisaoDto>();
        config.NewConfig<Revisao, RevisaoDto>();
        config.NewConfig<HistoricoDeRevisao, HistoricoDeRevisaoDto>();
    }
}
public sealed class RevisaoAppService(IRevisaoRepository revisoes, IRepository<PerguntaDeRevisao> perguntas, IRepository<HistoricoDeRevisao> historicos, IUnitOfWork unitOfWork, IUsuarioAtual atual, AprendizadoAppService aprendizados, IAgendadorDeRevisao agendador, TimeProvider tempo, TypeAdapterConfig config)
{
    public async Task<PerguntaAgendadaDto> CriarAsync(CriarPerguntaDto dto, CancellationToken ct)
    {
        await aprendizados.ExigirAprendizadoAsync(dto.IdAprendizado, ct);
        var pergunta = new PerguntaDeRevisao(atual.IdUsuario, dto.IdAprendizado, dto.Pergunta, dto.RespostaEsperada, dto.Origem);
        var revisao = new Revisao(atual.IdUsuario, pergunta.Id, tempo.GetUtcNow());
        await perguntas.AdicionarAsync(pergunta, ct);
        await revisoes.AdicionarAsync(revisao, ct);
        await unitOfWork.SalvarAsync(ct);
        return new(pergunta.Adapt<PerguntaDeRevisaoDto>(config), revisao.Adapt<RevisaoDto>(config));
    }
    public async Task<PerguntaDeRevisaoDto> ObterPerguntaAsync(Guid id, CancellationToken ct) =>
        ((await perguntas.ListarAsync(x => x.Id == id && x.IdUsuario == atual.IdUsuario, 1, ct)).FirstOrDefault() ?? throw new NaoEncontradoException()).Adapt<PerguntaDeRevisaoDto>(config);
    private async Task<Revisao> ObterEntidadeAsync(Guid id, CancellationToken ct) =>
        (await revisoes.ListarAsync(x => x.Id == id && x.IdUsuario == atual.IdUsuario, 1, ct)).FirstOrDefault() ?? throw new NaoEncontradoException();
    public async Task<RevisaoDto> ObterAsync(Guid id, CancellationToken ct) => (await ObterEntidadeAsync(id, ct)).Adapt<RevisaoDto>(config);
    public async Task<IReadOnlyList<RevisaoDto>> ListarDevidasAsync(Guid? idLivro, Guid? idConceito, int limite, CancellationToken ct)
    {
        if (limite is < 1 or > 500)
            throw new RegraDeDominioException("Limite deve estar entre 1 e 500.");
        return (await revisoes.ListarDevidasAsync(atual.IdUsuario, tempo.GetUtcNow(), idLivro, idConceito, limite, ct)).Select(x => x.Adapt<RevisaoDto>(config)).ToArray();
    }
    public async Task<RevisaoDto> RegistrarAsync(Guid id, RegistrarRevisaoDto dto, CancellationToken ct)
    {
        var revisao = await ObterEntidadeAsync(id, ct);
        var agora = tempo.GetUtcNow();
        var proxima = agendador.CalcularProxima(revisao, dto.Resultado, agora);
        var historico = revisao.Registrar(dto.Resultado, dto.Resposta, agora, proxima);
        await historicos.AdicionarAsync(historico, ct);
        await unitOfWork.SalvarAsync(ct);
        return revisao.Adapt<RevisaoDto>(config);
    }
    public async Task<IReadOnlyList<HistoricoDeRevisaoDto>> ListarHistoricoAsync(Guid id, int pagina, CancellationToken ct)
    {
        await ObterEntidadeAsync(id, ct);
        if (pagina is < 1 or > 1000000)
            throw new RegraDeDominioException("Página inválida.");
        return (await revisoes.ListarHistoricoAsync(atual.IdUsuario, id, pagina, ct)).Select(x => x.Adapt<HistoricoDeRevisaoDto>(config)).ToArray();
    }
}
