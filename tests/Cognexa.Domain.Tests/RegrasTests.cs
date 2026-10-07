using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Biblioteca;
using Cognexa.Domain.Compartilhado;
using Cognexa.Domain.Conhecimento;
using Cognexa.Domain.Revisoes;
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
    [Fact]
    public void AnotacaoPreservaTextoLiteralEPermiteComentarioSemTrecho()
    {
        const string trecho = "  Texto original\n com espaços  ";
        var anotacao = new Anotacao(Guid.NewGuid(), Guid.NewGuid(), null, TipoDeAnotacao.Citacao, trecho, null, 2, null, DateTimeOffset.UtcNow);
        Assert.Equal(trecho, anotacao.TrechoOriginal);
        anotacao.Atualizar(null, TipoDeAnotacao.Insight, null, "Meu comentário", null, null);
        Assert.Null(anotacao.TrechoOriginal);
        Assert.Throws<RegraDeDominioException>(() => anotacao.Atualizar(null, TipoDeAnotacao.Insight, null, "  ", null, null));
    }
    [Fact]
    public void AprendizadoExigeFonteEConservaVinculos()
    {
        Assert.Throws<RegraDeDominioException>(() => new Aprendizado(Guid.NewGuid(), "Ideia", [], [], DateTimeOffset.UtcNow));
        var fonte = (Guid.NewGuid(), Guid.NewGuid());
        var aprendizado = new Aprendizado(Guid.NewGuid(), "Síntese", [fonte, fonte], [], DateTimeOffset.UtcNow);
        Assert.Single(aprendizado.Fontes);
        aprendizado.Atualizar("Nova síntese");
        Assert.Equal(fonte.Item1, aprendizado.Fontes.Single().IdAnotacao);
    }
    [Fact]
    public void RelacaoNaoPodeApontarParaSiMesma()
    {
        var id = Guid.NewGuid();
        Assert.Throws<RegraDeDominioException>(() => new RelacaoEntreConceitos(Guid.NewGuid(), id, id, TipoDeRelacao.Complementa));
    }
    [Fact]
    public void RevisaoPreservaHistoricoERegrideAposErro()
    {
        var agora = DateTimeOffset.UtcNow;
        var revisao = new Revisao(Guid.NewGuid(), Guid.NewGuid(), agora);
        var historico = new List<HistoricoDeRevisao>();
        for (var i = 0; i < 6; i++)
        {
            historico.Add(revisao.Registrar(ResultadoDaRevisao.Bom, "Resposta", agora, agora.AddDays(1)));
            agora = agora.AddDays(1);
        }
        Assert.Equal(NivelDeDominio.Dominado, revisao.NivelDeDominio);
        historico.Add(revisao.Registrar(ResultadoDaRevisao.Errou, "Não lembrei", agora, agora.AddDays(1)));
        Assert.Equal(7, historico.Count);
        Assert.Equal(NivelDeDominio.Dominado, historico[^1].NivelAnterior);
        Assert.Equal(NivelDeDominio.Inicial, revisao.NivelDeDominio);
        Assert.Throws<ConflitoException>(() => revisao.Registrar(ResultadoDaRevisao.Bom, null, agora, agora.AddDays(1)));
    }
}
