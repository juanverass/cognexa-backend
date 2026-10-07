using Cognexa.Domain.Biblioteca;
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
    [Fact]
    public void LivroNaoIniciaLeituraAutomaticamente()
    {
        var livro = new Livro(Guid.NewGuid(), "Livro", ["Autor"], 100);
        var leitura = new Leitura(livro.IdUsuario, livro.Id);
        Assert.Equal(StatusDaLeitura.QueroLer, leitura.Status);
        Assert.Null(leitura.DataDeInicio);
    }
    [Fact]
    public void LeituraValidaLimitesEConclusao()
    {
        var leitura = new Leitura(Guid.NewGuid(), Guid.NewGuid());
        var agora = DateTimeOffset.UtcNow;
        Assert.Throws<RegraDeDominioException>(() => leitura.Atualizar(StatusDaLeitura.Lendo, 101, 100, agora));
        Assert.Throws<RegraDeDominioException>(() => leitura.Atualizar(StatusDaLeitura.Concluido, 99, 100, agora));
        Assert.Throws<ConflitoException>(() => leitura.Atualizar(StatusDaLeitura.Pausado, 0, 100, agora));
        leitura.Atualizar(StatusDaLeitura.Lendo, 20, 100, agora);
        leitura.Atualizar(StatusDaLeitura.Pausado, 20, 100, agora);
        Assert.Equal(agora, leitura.DataDeInicio);
        Assert.Throws<ConflitoException>(() => leitura.Atualizar(StatusDaLeitura.QueroLer, 0, 100, agora));
        leitura.Atualizar(StatusDaLeitura.Concluido, 100, 100, agora);
        Assert.Equal(agora, leitura.DataDeConclusao);
        leitura.Atualizar(StatusDaLeitura.Lendo, 90, 100, agora);
        Assert.Null(leitura.DataDeConclusao);
    }
}
