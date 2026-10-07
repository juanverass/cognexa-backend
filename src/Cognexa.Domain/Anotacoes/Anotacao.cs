using Cognexa.Domain.Compartilhado;
namespace Cognexa.Domain.Anotacoes;

public enum TipoDeAnotacao
{
    Destaque, Insight, Duvida, Citacao, Aplicacao
}
public sealed class Anotacao : EntidadeBase
{
    private Anotacao()
    {
    }
    public Anotacao(Guid idUsuario, Guid idLivro, Guid? idCapitulo, TipoDeAnotacao tipo, string? trechoOriginal, string? comentario, int? pagina, string? localizacao, DateTimeOffset agora)
    {
        IdUsuario = Validacao.Id(idUsuario, "IdUsuario");
        IdLivro = Validacao.Id(idLivro, "IdLivro");
        DataDeCriacao = agora;
        Atualizar(idCapitulo, tipo, trechoOriginal, comentario, pagina, localizacao);
    }
    public Guid IdUsuario
    {
        get; private set;
    }
    public Guid IdLivro
    {
        get; private set;
    }
    public Guid? IdCapitulo
    {
        get; private set;
    }
    public TipoDeAnotacao Tipo
    {
        get; private set;
    }
    public string? TrechoOriginal
    {
        get; private set;
    }
    public string? Comentario
    {
        get; private set;
    }
    public int? Pagina
    {
        get; private set;
    }
    public string? Localizacao
    {
        get; private set;
    }
    public DateTimeOffset DataDeCriacao
    {
        get; private set;
    }
    public void Atualizar(Guid? idCapitulo, TipoDeAnotacao tipo, string? trechoOriginal, string? comentario, int? pagina, string? localizacao)
    {
        if (!Enum.IsDefined(tipo) || pagina is <= 0)
            throw new RegraDeDominioException("Tipo ou página inválida.");
        if (idCapitulo.HasValue)
            Validacao.Id(idCapitulo.Value, "IdCapitulo");
        if (string.IsNullOrWhiteSpace(trechoOriginal) && string.IsNullOrWhiteSpace(comentario))
            throw new RegraDeDominioException("Informe trecho original ou comentário.");
        if (trechoOriginal?.Length > 20000 || comentario?.Length > 20000 || localizacao?.Length > 500)
            throw new RegraDeDominioException("Anotação excede o limite de tamanho.");
        // O trecho é preservado literalmente; não é normalizado nem reescrito.
        IdCapitulo = idCapitulo;
        Tipo = tipo;
        TrechoOriginal = trechoOriginal;
        Comentario = comentario;
        Pagina = pagina;
        Localizacao = localizacao;
    }
}
