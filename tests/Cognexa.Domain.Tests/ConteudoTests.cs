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
