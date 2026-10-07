using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Compartilhado;
using Npgsql;
using Microsoft.EntityFrameworkCore;
namespace Cognexa.Infrastructure.Persistencia;

public class CognexaDbContext(DbContextOptions<CognexaDbContext> options) : DbContext(options), IUnitOfWork
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(typeof(CognexaDbContext).Assembly);
    public async Task<int> SalvarAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException) { throw new ConflitoException("Recurso alterado por outra operação. Recarregue e tente novamente."); }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: "23503" or "23505" or "23001" })
        {
            throw new ConflitoException("Operação conflita com um recurso existente ou referenciado.");
        }
    }
}
