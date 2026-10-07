using Cognexa.Domain.Compartilhado;
namespace Cognexa.Domain.Tests;

public class IdentidadeTests
{
    private sealed class Entidade : EntidadeBase;
    [Fact]
    public void IdentidadeEUnicaENaoPodeSerAlteradaPublicamente()
    {
        Assert.NotEqual(Guid.Empty, new Entidade().Id);
        Assert.NotEqual(new Entidade().Id, new Entidade().Id);
        Assert.False(typeof(EntidadeBase).GetProperty("Id")!.SetMethod!.IsPublic);
    }
}
