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
        services.AddScoped<Cognexa.Application.Usuarios.UsuarioAppService>();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<Cognexa.Application.Biblioteca.LivroAppService>();
        services.AddScoped<Cognexa.Application.Anotacoes.AnotacaoAppService>();
        services.AddScoped<Cognexa.Application.Conhecimento.ConceitoAppService>();
        services.AddScoped<Cognexa.Application.Conhecimento.AprendizadoAppService>();
        services.AddScoped<Cognexa.Application.Conhecimento.ConexoesAppService>();
        services.AddScoped<Cognexa.Application.Revisoes.RevisaoAppService>();
        return services;
    }
}
