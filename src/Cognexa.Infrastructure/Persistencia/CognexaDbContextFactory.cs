using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace Cognexa.Infrastructure.Persistencia;

public sealed class CognexaDbContextFactory : IDesignTimeDbContextFactory<CognexaDbContext>
{
    public CognexaDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__Cognexa");
        // Gerar migrations não requer conexão; aplicar ao banco requer configuração explícita.
        return new(new DbContextOptionsBuilder<CognexaDbContext>().UseNpgsql(connection).Options);
    }
}
