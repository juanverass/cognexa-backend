using System.Linq.Expressions;
using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Compartilhado;
namespace Cognexa.Application.Tests;

public class RepositoryEmMemoria<TEntity> : IRepository<TEntity> where TEntity : EntidadeBase
{
    public List<TEntity> Entidades { get; } = [];
    public Task<TEntity?> ObterAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Entidades.Find(x => x.Id == id));
    public Task<IReadOnlyList<TEntity>> ListarAsync(Expression<Func<TEntity, bool>> filtro, int limite = 100, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<TEntity>>(Entidades.Where(filtro.Compile()).Take(limite).ToArray());
    public Task AdicionarAsync(TEntity entidade, CancellationToken cancellationToken = default)
    {
        Entidades.Add(entidade);
        return Task.CompletedTask;
    }
    public void Remover(TEntity entidade) => Entidades.Remove(entidade);
}

public sealed class UsuarioAtualTeste : IUsuarioAtual
{
    public Guid IdUsuario { get; set; } = Guid.NewGuid();
}

public sealed class UnitOfWorkTeste : IUnitOfWork
{
    public int Gravacoes
    {
        get; private set;
    }
    public Task<int> SalvarAsync(CancellationToken cancellationToken = default) => Task.FromResult(++Gravacoes);
}

public sealed class TempoTeste : TimeProvider
{
    public DateTimeOffset Agora { get; set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => Agora;
}

public sealed class VinculoTeste(UsuarioAtualTeste atual) : Cognexa.Application.Usuarios.IIdentidadeAtual, Cognexa.Application.Usuarios.IVinculoDeIdentidadeRepository
{
    private readonly Dictionary<Guid, Guid> _usuarios = [];
    public Guid IdIdentidade { get; } = Guid.NewGuid();
    public Task<Guid?> ObterIdUsuarioAsync(Guid idIdentidade, CancellationToken cancellationToken) => Task.FromResult<Guid?>(_usuarios.TryGetValue(idIdentidade, out var id) ? id : null);
    public Task AssociarAsync(Guid idIdentidade, Guid idUsuario, CancellationToken cancellationToken)
    {
        _usuarios.Add(idIdentidade, idUsuario);
        atual.IdUsuario = idUsuario;
        return Task.CompletedTask;
    }
}
