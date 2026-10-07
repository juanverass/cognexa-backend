using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Conhecimento;
using Mapster;
namespace Cognexa.Application.Conhecimento;

public sealed class ConexoesAppService(IRepository<AplicacaoPratica> aplicacoes, IRepository<RelacaoEntreConceitos> relacoes, IUnitOfWork unitOfWork, IUsuarioAtual atual, AprendizadoAppService aprendizados, ConceitoAppService conceitos, TypeAdapterConfig config)
{
    public async Task<AplicacaoPraticaDto> CriarAplicacaoAsync(SalvarAplicacaoPraticaDto dto, CancellationToken ct)
    {
        await aprendizados.ExigirAprendizadoAsync(dto.IdAprendizado, ct);
        var aplicacao = new AplicacaoPratica(atual.IdUsuario, dto.IdAprendizado, dto.Descricao, dto.Origem);
        await aplicacoes.AdicionarAsync(aplicacao, ct);
        await unitOfWork.SalvarAsync(ct);
        return aplicacao.Adapt<AplicacaoPraticaDto>(config);
    }
    public async Task<IReadOnlyList<AplicacaoPraticaDto>> ListarAplicacoesAsync(Guid? idAprendizado, CancellationToken ct) =>
        (await aplicacoes.ListarAsync(x => x.IdUsuario == atual.IdUsuario && (!idAprendizado.HasValue || x.IdAprendizado == idAprendizado), 500, ct)).Select(x => x.Adapt<AplicacaoPraticaDto>(config)).ToArray();
    public async Task<AplicacaoPraticaDto> AtualizarAplicacaoAsync(Guid id, AtualizarConteudoDto dto, CancellationToken ct)
    {
        var aplicacao = (await aplicacoes.ListarAsync(x => x.Id == id && x.IdUsuario == atual.IdUsuario, 1, ct)).FirstOrDefault() ?? throw new NaoEncontradoException();
        aplicacao.Atualizar(dto.Conteudo);
        await unitOfWork.SalvarAsync(ct);
        return aplicacao.Adapt<AplicacaoPraticaDto>(config);
    }
    public async Task RemoverAplicacaoAsync(Guid id, CancellationToken ct)
    {
        var aplicacao = (await aplicacoes.ListarAsync(x => x.Id == id && x.IdUsuario == atual.IdUsuario, 1, ct)).FirstOrDefault() ?? throw new NaoEncontradoException();
        aplicacoes.Remover(aplicacao);
        await unitOfWork.SalvarAsync(ct);
    }
    public async Task<RelacaoEntreConceitosDto> CriarRelacaoAsync(CriarRelacaoDto dto, CancellationToken ct)
    {
        await conceitos.ObterAsync(dto.IdConceitoOrigem, ct);
        await conceitos.ObterAsync(dto.IdConceitoDestino, ct);
        var relacao = new RelacaoEntreConceitos(atual.IdUsuario, dto.IdConceitoOrigem, dto.IdConceitoDestino, dto.Tipo);
        await relacoes.AdicionarAsync(relacao, ct);
        await unitOfWork.SalvarAsync(ct);
        return relacao.Adapt<RelacaoEntreConceitosDto>(config);
    }
    public async Task<IReadOnlyList<RelacaoEntreConceitosDto>> ListarRelacoesAsync(Guid? idConceito, CancellationToken ct) =>
        (await relacoes.ListarAsync(x => x.IdUsuario == atual.IdUsuario && (!idConceito.HasValue || x.IdConceitoOrigem == idConceito || x.IdConceitoDestino == idConceito), 500, ct)).Select(x => x.Adapt<RelacaoEntreConceitosDto>(config)).ToArray();
    public async Task RemoverRelacaoAsync(Guid id, CancellationToken ct)
    {
        var relacao = (await relacoes.ListarAsync(x => x.Id == id && x.IdUsuario == atual.IdUsuario, 1, ct)).FirstOrDefault() ?? throw new NaoEncontradoException();
        relacoes.Remover(relacao);
        await unitOfWork.SalvarAsync(ct);
    }
}
