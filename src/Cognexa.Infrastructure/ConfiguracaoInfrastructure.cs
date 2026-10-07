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
        services.AddOptions<Cognexa.Application.Anotacoes.ConteudoOptions>().Bind(configuration.GetSection("Conteudo"));
        services.AddSingleton(p => p.GetRequiredService<Microsoft.Extensions.Options.IOptions<Cognexa.Application.Anotacoes.ConteudoOptions>>().Value);
        services.AddScoped<Cognexa.Application.Anotacoes.IControleDeConteudo, ControleDeConteudo>();
        services.AddScoped<Cognexa.Application.Anotacoes.PublicacaoAppService>();
        services.AddScoped<Cognexa.Application.Anotacoes.OcrAppService>();
        services.AddSingleton<Cognexa.Application.Anotacoes.IOcrTemporario, Cognexa.Infrastructure.Inteligencia.OcrTemporario>();
        services.AddHttpClient<Cognexa.Application.Anotacoes.IOcrProvider, Cognexa.Infrastructure.Inteligencia.OcrProvider>(client =>
        {
            var url = configuration["Ocr:BaseUrl"];
            if (url != null)
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != "https") throw new InvalidOperationException("OCR exige HTTPS.");
                client.BaseAddress = new Uri(uri.AbsoluteUri.TrimEnd('/') + "/");
            }
            client.Timeout = TimeSpan.FromSeconds(30);
            var token = configuration["Ocr:Token"];
            if (!string.IsNullOrWhiteSpace(token)) client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        }).RemoveAllLoggers();
        services.AddDbContext<CognexaDbContext>(options => options.UseNpgsql(configuration.GetConnectionString("Cognexa")));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork>(p => p.GetRequiredService<CognexaDbContext>());
        services.AddHealthChecks().AddCheck<SaudeDoBanco>("postgresql", tags: ["ready"]);
        services.AddScoped<Cognexa.Application.Revisoes.IRevisaoRepository, Cognexa.Infrastructure.Revisoes.RevisaoRepository>();
        services.AddSingleton<Cognexa.Application.Revisoes.IAgendadorDeRevisao, Cognexa.Infrastructure.Revisoes.AgendadorDeRevisao>();
        var protocolo = configuration["Inteligencia:Protocolo"] ?? "compativel";
        if (protocolo is not ("compativel" or "gateway"))
            throw new InvalidOperationException("Protocolo de inteligência inválido.");
        services.AddOptions<Cognexa.Infrastructure.Inteligencia.InteligenciaOptions>().Bind(configuration.GetSection("Inteligencia"));
        void ConfigurarHttp(HttpClient client)
        {
            var url = configuration["Inteligencia:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(url))
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != "https")
                    throw new InvalidOperationException("Provider deve usar uma URL HTTPS válida.");
                client.BaseAddress = new Uri(uri.AbsoluteUri.TrimEnd('/') + "/");
            }
            client.Timeout = TimeSpan.FromSeconds(60);
            var token = configuration["Inteligencia:Token"];
            if (!string.IsNullOrWhiteSpace(token))
                client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        }
        services.AddHttpClient<Cognexa.Infrastructure.Inteligencia.ProviderHttp>(ConfigurarHttp);
        services.AddHttpClient<Cognexa.Infrastructure.Inteligencia.ProviderCompativel>(ConfigurarHttp);
        services.AddScoped<Cognexa.Application.Inteligencia.IInteligenciaProvider>(p => protocolo == "gateway"
            ? p.GetRequiredService<Cognexa.Infrastructure.Inteligencia.ProviderHttp>()
            : p.GetRequiredService<Cognexa.Infrastructure.Inteligencia.ProviderCompativel>());
        services.AddScoped<Cognexa.Infrastructure.Inteligencia.IVetorizador>(p => (Cognexa.Infrastructure.Inteligencia.IVetorizador)p.GetRequiredService<Cognexa.Application.Inteligencia.IInteligenciaProvider>());
        services.AddScoped<Cognexa.Application.Inteligencia.IBuscaSemanticaProvider, Cognexa.Infrastructure.Inteligencia.BuscaSemanticaProvider>();
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
