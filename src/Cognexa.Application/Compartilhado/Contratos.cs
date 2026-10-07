using System.Linq.Expressions;
using Cognexa.Domain.Compartilhado;
namespace Cognexa.Application.Compartilhado;

public interface IRepository<TEntity> where TEntity : EntidadeBase
{
    Task<TEntity?> ObterAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListarAsync(Expression<Func<TEntity, bool>> filtro, int limite = 100, CancellationToken cancellationToken = default);
    Task AdicionarAsync(TEntity entidade, CancellationToken cancellationToken = default);
    void Remover(TEntity entidade);
}
public interface IUnitOfWork
{
    Task<int> SalvarAsync(CancellationToken cancellationToken = default);
}
public interface IUsuarioAtual
{
    Guid IdUsuario
    {
        get;
    }
}
public class NaoEncontradoException() : Exception("Recurso não encontrado.");
public class ServicoIndisponivelException(string mensagem) : Exception(mensagem);
public record SearchDto(int Limite = 100);
public interface ICrudBasicoAppService<TDto, TSearchDto, TEntity> where TSearchDto : SearchDto where TEntity : EntidadeBase
{
    Task<TDto> CriarAsync(TDto dto, CancellationToken cancellationToken = default);
    Task<TDto> AtualizarAsync(Guid id, TDto dto, CancellationToken cancellationToken = default);
    Task<TDto> ObterAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TDto>> ListarAsync(TSearchDto busca, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
public abstract class CrudBasicoAppService<TDto, TSearchDto, TEntity>(IRepository<TEntity> repository, IUnitOfWork unitOfWork)
    : ICrudBasicoAppService<TDto, TSearchDto, TEntity> where TSearchDto : SearchDto where TEntity : EntidadeBase
{
    protected abstract Task<TDto> CriarDtoAsync(TDto dto, CancellationToken cancellationToken);
    protected abstract Task<TDto> AtualizarDtoAsync(Guid id, TDto dto, CancellationToken cancellationToken);
    Task<TDto> ICrudBasicoAppService<TDto, TSearchDto, TEntity>.CriarAsync(TDto dto, CancellationToken cancellationToken) => CriarDtoAsync(dto, cancellationToken);
    Task<TDto> ICrudBasicoAppService<TDto, TSearchDto, TEntity>.AtualizarAsync(Guid id, TDto dto, CancellationToken cancellationToken) => AtualizarDtoAsync(id, dto, cancellationToken);
    protected IRepository<TEntity> Repository { get; } = repository;
    protected IUnitOfWork UnitOfWork { get; } = unitOfWork;
    protected abstract Expression<Func<TEntity, bool>> Filtro(TSearchDto busca);
    protected abstract Expression<Func<TEntity, bool>> FiltroPorId(Guid id);
    protected abstract TDto Converter(TEntity entidade);
    protected virtual Task AntesDeRemoverAsync(TEntity entidade, CancellationToken cancellationToken) => Task.CompletedTask;
    protected async Task<TEntity> ObterEntidadeAsync(Guid id, CancellationToken cancellationToken) =>
        (await Repository.ListarAsync(FiltroPorId(id), 1, cancellationToken)).FirstOrDefault() ?? throw new NaoEncontradoException();
    public async Task<TDto> ObterAsync(Guid id, CancellationToken cancellationToken = default) => Converter(await ObterEntidadeAsync(id, cancellationToken));
    public async Task<IReadOnlyList<TDto>> ListarAsync(TSearchDto busca, CancellationToken cancellationToken = default) =>
        (await Repository.ListarAsync(Filtro(busca), busca.Limite, cancellationToken)).Select(Converter).ToArray();
    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entidade = await ObterEntidadeAsync(id, cancellationToken);
        await AntesDeRemoverAsync(entidade, cancellationToken);
        Repository.Remover(entidade);
        await UnitOfWork.SalvarAsync(cancellationToken);
    }
}
