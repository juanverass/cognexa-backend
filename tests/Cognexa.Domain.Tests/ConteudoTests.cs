using Cognexa.Domain.Anotacoes;
using Cognexa.Domain.Compartilhado;
namespace Cognexa.Domain.Tests;

public class ConteudoTests
{
    private static Anotacao Criar() => new(Guid.NewGuid(), Guid.NewGuid(), null, TipoDeAnotacao.Citacao, "citação", "interpretação", 1, null, DateTimeOffset.UtcNow);
    [Fact]
    public void PublicacaoExigeAtribuicaoEGate()
    {
        var a = Criar();
        Assert.Equal(EstadoDePublicacao.Privado, a.Publicacao);
        Assert.Throws<RegraDeDominioException>(a.PrepararPublicacao);
        a.DefinirProveniencia(MetodoDeCaptura.Ocr, OrigemDoConteudo.InteligenciaArtificial, "Obra", "Autor");
        a.PrepararPublicacao();
        Assert.Throws<RegraDeDominioException>(() => a.Publicar(false));
        a.Publicar(true);
        a.Atualizar(null, TipoDeAnotacao.Citacao, "outro", "comentário", 2, null);
        Assert.Equal(EstadoDePublicacao.Privado, a.Publicacao);
    }
    [Fact]
    public void RetiradaNaoPodeSerReexpostaPorEdicao()
    {
        var a = Criar();
        a.DefinirProveniencia(MetodoDeCaptura.Manual, OrigemDoConteudo.Manual, "Obra", "Autor");
        a.Publicar(true);
        a.Denunciar();
        a.Retirar();
        a.Atualizar(null, TipoDeAnotacao.Citacao, "outro", "texto", 1, null);
        Assert.Equal(EstadoDePublicacao.Removido, a.Publicacao);
        Assert.Throws<RegraDeDominioException>(() => a.Publicar(true));
    }
}

public class ChaveDeObraTests
{
    [Fact]
    public void RecadastrosEEdicoesCompartilhamChaveSemDependerDeIsbn()
    {
        var usuario = Guid.NewGuid();
        var original = new Cognexa.Domain.Biblioteca.Livro(usuario, "Ação: e Reflexão", ["João", "Maria"], 100, "9780000000001", "1");
        var edicao = new Cognexa.Domain.Biblioteca.Livro(usuario, "  ACAO E REFLEXAO! ", ["MARIA", "joa\u0303o", "João"], 200, "9780000000002", "2");
        var semIsbn = new Cognexa.Domain.Biblioteca.Livro(usuario, "Ação e reflexão", ["Maria", "João"]);
        Assert.Equal(original.ChaveDaObra, edicao.ChaveDaObra);
        Assert.Equal(original.ChaveDaObra, semIsbn.ChaveDaObra);
        Assert.NotEqual(original.ChaveDaObra, Cognexa.Domain.Biblioteca.ChaveDeObra.Criar("Outra obra", ["João", "Maria"]));
        Assert.NotEqual(original.ChaveDaObra, Cognexa.Domain.Biblioteca.ChaveDeObra.Criar("Ação e reflexão", ["Outro autor"]));
        var chave = original.ChaveDaObra;
        original.Atualizar("Correção de título", ["Autor corrigido"], 100, null, null, null);
        Assert.Equal(chave, original.ChaveDaObra);
    }
}
