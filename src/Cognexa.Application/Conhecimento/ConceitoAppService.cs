using System.Linq.Expressions;
using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Conhecimento;
using Mapster;
namespace Cognexa.Application.Conhecimento;

public sealed class ConceitoAppService(IRepository<Conceito> conceitos, IUnitOfWork unitOfWork, IUsuarioAtual atual, TypeAdapterConfig config)
    : CrudBasicoAppService<ConceitoDto, SearchDto, Conceito>(conceitos, unitOfWork)
{
    protected override Task<ConceitoDto> CriarDtoAsync(ConceitoDto dto, CancellationToken cancellationToken) => CriarAsync(new SalvarConceitoDto(dto.Nome), cancellationToken);
    protected override Task<ConceitoDto> AtualizarDtoAsync(Guid id, ConceitoDto dto, CancellationToken cancellationToken) => AtualizarAsync(id, new SalvarConceitoDto(dto.Nome), cancellationToken);

    protected override Expression<Func<Conceito, bool>> Filtro(SearchDto busca) => x => x.IdUsuario == atual.IdUsuario;
    protected override Expression<Func<Conceito, bool>> FiltroPorId(Guid id) => x => x.Id == id && x.IdUsuario == atual.IdUsuario;
    protected override ConceitoDto Converter(Conceito entidade) => entidade.Adapt<ConceitoDto>(config);
    public async Task<ConceitoDto> CriarAsync(SalvarConceitoDto dto, CancellationToken ct)
    {
        var conceito = new Conceito(atual.IdUsuario, dto.Nome);
        await Repository.AdicionarAsync(conceito, ct);
        await UnitOfWork.SalvarAsync(ct);
        return Converter(conceito);
    }
    public async Task<ConceitoDto> AtualizarAsync(Guid id, SalvarConceitoDto dto, CancellationToken ct)
    {
        var conceito = await ObterEntidadeAsync(id, ct);
        conceito.Atualizar(dto.Nome);
        await UnitOfWork.SalvarAsync(ct);
        return Converter(conceito);
    }
}
