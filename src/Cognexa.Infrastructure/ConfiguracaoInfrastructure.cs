using Cognexa.Application.Compartilhado;
using Cognexa.Infrastructure.Persistencia;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
namespace Cognexa.Infrastructure;

public static class ConfiguracaoInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CognexaDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Cognexa")));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork>(p => p.GetRequiredService<CognexaDbContext>());
        services.AddHealthChecks().AddCheck<SaudeDoBanco>("postgresql", tags: ["ready"]);
        services.AddScoped<Cognexa.Application.Usuarios.IVinculoDeIdentidadeRepository, Cognexa.Infrastructure.Identidade.VinculoDeIdentidadeRepository>();
        return services;
    }
}
internal sealed class SaudeDoBanco(IServiceScopeFactory scopes) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CognexaDbContext>();
        try
        {
            return await db.Database.CanConnectAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy("Banco indisponível.");
        }
        catch (Exception) { return HealthCheckResult.Unhealthy("Banco indisponível."); }
    }
}
