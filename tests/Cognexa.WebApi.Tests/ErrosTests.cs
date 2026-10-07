using System.Net;
using System.Net.Http.Json;
using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Compartilhado;
using Cognexa.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace Cognexa.WebApi.Tests;

public class ErrosTests
{
    [Theory]
    [InlineData(400)]
    [InlineData(404)]
    [InlineData(409)]
    [InlineData(500)]
    public async Task ErrosRetornamProblemDetailsSemDetalhesInternos(int status)
    {
        using var host = await new HostBuilder().ConfigureWebHost(web => web.UseTestServer().ConfigureServices(services =>
        {
            services.AddRouting();
            services.AddProblemDetails();
            services.AddExceptionHandler<TratamentoDeErros>();
        }).Configure(app =>
        {
            app.UseExceptionHandler();
            app.Run(_ => throw status switch
            {
                400 => new RegraDeDominioException("Campo inválido."),
                404 => new NaoEncontradoException(),
                409 => new ConflitoException("Operação em conflito."),
                _ => new InvalidOperationException("Password=secret SQL SELECT internal")
            });
        })).StartAsync();
        var response = await host.GetTestClient().GetAsync("/");
        Assert.Equal((HttpStatusCode)status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var body = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(status, body!.Status);
        Assert.True(body.Extensions.ContainsKey("traceId"));
        Assert.DoesNotContain("secret", await response.Content.ReadAsStringAsync());
        if (status == 500)
            Assert.Null(body.Detail);
    }
}
