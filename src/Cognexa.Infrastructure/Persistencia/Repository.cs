using System.Linq.Expressions;
using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Compartilhado;
using Microsoft.EntityFrameworkCore;
namespace Cognexa.Infrastructure.Persistencia;

public class Repository<TEntity>(CognexaDbContext contexto) : IRepository<TEntity> where TEntity : EntidadeBase
{
    protected CognexaDbContext Contexto { get; } = contexto;
    public async Task<TEntity?> ObterAsync(Guid id, CancellationToken cancellationToken = default) =>
        await Contexto.Set<TEntity>().FindAsync([id], cancellationToken);
    public async Task<IReadOnlyList<TEntity>> ListarAsync(Expression<Func<TEntity, bool>> filtro, int limite = 100, CancellationToken cancellationToken = default)
    {
        if (limite is < 1 or > 500)
            throw new RegraDeDominioException("Limite deve estar entre 1 e 500.");
        return await Contexto.Set<TEntity>().Where(filtro).OrderBy(e => e.Id).Take(limite).ToListAsync(cancellationToken);
    }
    public async Task AdicionarAsync(TEntity entidade, CancellationToken cancellationToken = default) => await Contexto.Set<TEntity>().AddAsync(entidade, cancellationToken);
    public void Remover(TEntity entidade) => Contexto.Set<TEntity>().Remove(entidade);
}
