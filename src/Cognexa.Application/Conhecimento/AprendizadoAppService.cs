using System.Linq.Expressions;
using Cognexa.Application.Anotacoes;
using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Compartilhado;
using Cognexa.Domain.Conhecimento;
using Mapster;
namespace Cognexa.Application.Conhecimento;

public sealed class AprendizadoAppService(IRepository<Aprendizado> aprendizados, IUnitOfWork unitOfWork, IUsuarioAtual atual, AnotacaoAppService anotacoes, ConceitoAppService conceitos, TypeAdapterConfig config, TimeProvider tempo)
    : CrudBasicoAppService<AprendizadoDto, AprendizadoSearchDto, Aprendizado>(aprendizados, unitOfWork)
{
    protected override Task<AprendizadoDto> CriarDtoAsync(AprendizadoDto dto, CancellationToken cancellationToken) => CriarAsync(new CriarAprendizadoDto(dto.Conteudo, dto.Fontes?.Select(f => f.IdAnotacao).ToArray() ?? [], dto.IdConceitos, dto.Origem), cancellationToken);
    protected override Task<AprendizadoDto> AtualizarDtoAsync(Guid id, AprendizadoDto dto, CancellationToken cancellationToken) => AtualizarAsync(id, new AtualizarConteudoDto(dto.Conteudo), cancellationToken);

    protected override Expression<Func<Aprendizado, bool>> Filtro(AprendizadoSearchDto busca) => x => x.IdUsuario == atual.IdUsuario
        && (!busca.IdLivro.HasValue || x.Fontes.Any(f => f.IdLivro == busca.IdLivro))
        && (!busca.IdConceito.HasValue || x.Conceitos.Any(c => c.IdConceito == busca.IdConceito))
        && (!busca.IdAnotacao.HasValue || x.Fontes.Any(f => f.IdAnotacao == busca.IdAnotacao));
    protected override Expression<Func<Aprendizado, bool>> FiltroPorId(Guid id) => x => x.Id == id && x.IdUsuario == atual.IdUsuario;
    protected override AprendizadoDto Converter(Aprendizado entidade) => entidade.Adapt<AprendizadoDto>(config);
    public Task<Aprendizado> ExigirAprendizadoAsync(Guid id, CancellationToken ct) => ObterEntidadeAsync(id, ct);
    public async Task<AprendizadoDto> CriarAsync(CriarAprendizadoDto dto, CancellationToken ct)
    {
        if (dto.IdAnotacoes is null || dto.IdAnotacoes.Length is < 1 or > 100 || dto.IdConceitos is null || dto.IdConceitos.Length > 100)
            throw new RegraDeDominioException("Fontes ou conceitos inválidos.");
        var fontes = new List<(Guid, Guid)>();
        foreach (var id in dto.IdAnotacoes.Distinct())
        {
            var origem = await anotacoes.ExigirAnotacaoAsync(id, ct);
            fontes.Add((origem.Id, origem.IdLivro));
        }
        foreach (var id in dto.IdConceitos.Distinct())
            await conceitos.ObterAsync(id, ct);
        var aprendizado = new Aprendizado(atual.IdUsuario, dto.Conteudo, fontes, dto.IdConceitos, tempo.GetUtcNow(), dto.Origem);
        await Repository.AdicionarAsync(aprendizado, ct);
        await UnitOfWork.SalvarAsync(ct);
        return Converter(aprendizado);
    }
    public async Task<AprendizadoDto> AtualizarAsync(Guid id, AtualizarConteudoDto dto, CancellationToken ct)
    {
        var aprendizado = await ExigirAprendizadoAsync(id, ct);
        aprendizado.Atualizar(dto.Conteudo);
        await UnitOfWork.SalvarAsync(ct);
        return Converter(aprendizado);
    }
}
