using Mapster;
using Microsoft.Extensions.DependencyInjection;
namespace Cognexa.Application;

public static class ConfiguracaoApplication
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var config = new TypeAdapterConfig();
        config.Scan(typeof(ConfiguracaoApplication).Assembly);
        services.AddSingleton(config);
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
