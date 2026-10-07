using Cognexa.Application;
using Cognexa.Application.Compartilhado;
using Cognexa.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Cognexa.Infrastructure.Tests;

public class ConfiguracaoTests
{
    [Fact]
    public void RegistraPostgreSqlEUnitOfWorkNoMesmoEscopo()
    {
        using var services = new ServiceCollection().AddApplication().AddInfrastructure(new ConfigurationBuilder().Build()).BuildServiceProvider();
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CognexaDbContext>();
        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", db.Database.ProviderName);
        Assert.Same(db, scope.ServiceProvider.GetRequiredService<IUnitOfWork>());
    }
}
