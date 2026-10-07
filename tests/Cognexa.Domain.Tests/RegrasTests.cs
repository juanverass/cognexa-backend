using Cognexa.Domain.Compartilhado;
using Cognexa.Domain.Usuarios;
namespace Cognexa.Domain.Tests;

public class RegrasTests
{
    [Fact]
    public void ContaDesativadaNaoAceitaAlteracoes()
    {
        var usuario = new Usuario("João");
        usuario.Desativar();
        Assert.Throws<ConflitoException>(() => usuario.Atualizar("Outro"));
        Assert.Throws<ConflitoException>(() => usuario.AtualizarPreferencias(30, "pt-BR"));
    }
    [Theory]
    [InlineData(0)]
    [InlineData(1441)]
    public void PreferenciasValidamMeta(int meta) => Assert.Throws<RegraDeDominioException>(() => new PreferenciasDoUsuario(meta, "pt-BR"));
}
