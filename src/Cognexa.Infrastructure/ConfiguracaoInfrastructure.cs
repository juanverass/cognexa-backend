using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Cognexa.Infrastructure;
public static class ConfiguracaoInfrastructure
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) => services;
}
