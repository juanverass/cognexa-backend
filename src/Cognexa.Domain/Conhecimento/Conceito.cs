using Cognexa.Domain.Compartilhado;
namespace Cognexa.Domain.Conhecimento;

public sealed class Conceito : EntidadeBase
{
    private Conceito()
    {
    }
    public Conceito(Guid idUsuario, string nome)
    {
        IdUsuario = Validacao.Id(idUsuario, "IdUsuario");
        Atualizar(nome);
    }
    public Guid IdUsuario
    {
        get; private set;
    }
    public string Nome { get; private set; } = "";
    public void Atualizar(string nome) => Nome = Validacao.Texto(nome, "Nome", 200);
}
public enum TipoDeRelacao
{
    Complementa, Contradiz, Exemplifica, DependeDe
}
public sealed class RelacaoEntreConceitos : EntidadeBase
{
    private RelacaoEntreConceitos()
    {
    }
    public RelacaoEntreConceitos(Guid idUsuario, Guid idConceitoOrigem, Guid idConceitoDestino, TipoDeRelacao tipo)
    {
        IdUsuario = Validacao.Id(idUsuario, "IdUsuario");
        IdConceitoOrigem = Validacao.Id(idConceitoOrigem, "IdConceitoOrigem");
        IdConceitoDestino = Validacao.Id(idConceitoDestino, "IdConceitoDestino");
        if (idConceitoOrigem == idConceitoDestino || !Enum.IsDefined(tipo))
            throw new RegraDeDominioException("Relação inválida.");
        Tipo = tipo;
    }
    public Guid IdUsuario
    {
        get; private set;
    }
    public Guid IdConceitoOrigem
    {
        get; private set;
    }
    public Guid IdConceitoDestino
    {
        get; private set;
    }
    public TipoDeRelacao Tipo
    {
        get; private set;
    }
}
public sealed class AplicacaoPratica : EntidadeBase
{
    private AplicacaoPratica()
    {
    }
    public AplicacaoPratica(Guid idUsuario, Guid idAprendizado, string descricao, OrigemDoConteudo origem = OrigemDoConteudo.Manual)
    {
        if (!Enum.IsDefined(origem))
            throw new RegraDeDominioException("Origem inválida.");
        Origem = origem;
        IdUsuario = Validacao.Id(idUsuario, "IdUsuario");
        IdAprendizado = Validacao.Id(idAprendizado, "IdAprendizado");
        Atualizar(descricao);
    }
    public Guid IdUsuario
    {
        get; private set;
    }
    public Guid IdAprendizado
    {
        get; private set;
    }
    public OrigemDoConteudo Origem
    {
        get; private set;
    }
    public string Descricao { get; private set; } = "";
    public void Atualizar(string descricao) => Descricao = Validacao.Texto(descricao, "Descrição", 20000);
}
