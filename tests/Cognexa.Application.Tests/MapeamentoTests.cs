using Cognexa.Application;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
namespace Cognexa.Application.Tests;

public class MapeamentoTests
{
    public record Exemplo(Guid Id, string Nome);
    public record ExemploDto(Guid Id, string Nome);
    [Fact]
    public void ConfiguracaoRegistraMapsterSemAlterarIdentidade()
    {
        using var services = new ServiceCollection().AddApplication().BuildServiceProvider();
        var config = services.GetRequiredService<TypeAdapterConfig>();
        var origem = new Exemplo(Guid.NewGuid(), "Exemplo");
        Assert.Equal(new ExemploDto(origem.Id, origem.Nome), origem.Adapt<ExemploDto>(config));
    }
}
