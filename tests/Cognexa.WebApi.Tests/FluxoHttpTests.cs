using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cognexa.Application.Biblioteca;
using Cognexa.Application.Usuarios;
using Cognexa.Domain.Biblioteca;
using Cognexa.Domain.Compartilhado;
using Cognexa.Tests.Compartilhado;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Cognexa.WebApi.Tests;

public class FluxoHttpTests(PostgreSqlFixture banco) : IClassFixture<PostgreSqlFixture>
{
    private const string TokenA = "token-de-teste-usuario-a-1234567890";
    private const string TokenB = "token-de-teste-usuario-b-1234567890";
    private sealed class TempoControlado : TimeProvider
    {
        public DateTimeOffset Agora { get; set; } = DateTimeOffset.UtcNow;
        public override DateTimeOffset GetUtcNow() => Agora;
    }
    private WebApplicationFactory<Program> Factory(TempoControlado tempo) => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.UseEnvironment("Production");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Cognexa"] = banco.ConnectionString,
            ["Autenticacao:Identidades:0:IdIdentidade"] = Guid.NewGuid().ToString(),
            ["Autenticacao:Identidades:0:Token"] = TokenA,
            ["Autenticacao:Identidades:1:IdIdentidade"] = Guid.NewGuid().ToString(),
            ["Autenticacao:Identidades:1:Token"] = TokenB
        }));
        builder.ConfigureServices(services => services.AddSingleton<TimeProvider>(tempo));
    });
    private static async Task<T> Ler<T>(HttpResponseMessage response)
    {
        Assert.True(response.IsSuccessStatusCode, $"{response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }
    [PostgreSqlFact]
    public async Task AutenticacaoErrosDeEntradaESaudeRetornamContratosSeguros()
    {
        await using var factory = Factory(new TempoControlado());
        using var client = factory.CreateClient();
        var semToken = await client.GetAsync("/usuarios/me");
        Assert.Equal(HttpStatusCode.Unauthorized, semToken.StatusCode);
        Assert.Equal("application/problem+json", semToken.Content.Headers.ContentType!.MediaType);
        client.DefaultRequestHeaders.Authorization = new("Bearer", new string('x', 32));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/usuarios/me")).StatusCode);
        client.DefaultRequestHeaders.Authorization = new("Bearer", TokenA);
        var invalido = await client.PostAsJsonAsync("/usuarios/me", new SalvarUsuarioDto("  "));
        Assert.Equal(HttpStatusCode.BadRequest, invalido.StatusCode);
        var problem = await invalido.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.True(problem!.Extensions.ContainsKey("traceId"));
        var malformed = await client.PostAsync("/usuarios/me", new StringContent("{", System.Text.Encoding.UTF8, "application/json"));
        Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);
        Assert.Equal("application/problem+json", malformed.Content.Headers.ContentType!.MediaType);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/live")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/health/ready")).StatusCode);
    }
}
