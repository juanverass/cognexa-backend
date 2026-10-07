using Cognexa.Domain.Compartilhado;
namespace Cognexa.Domain.Anotacoes;

public enum TipoDeAnotacao
{
    Destaque, Insight, Duvida, Citacao, Aplicacao
}
public enum MetodoDeCaptura { Manual, Ocr, ImportacaoAutorizada }
public enum EstadoDePublicacao { Privado, Publicavel, Publicado, Removido }
public enum EstadoDeModeracao { SemDenuncia, Denunciado, Retirado }
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
    public MetodoDeCaptura MetodoDeCaptura { get; private set; }
    public OrigemDoConteudo OrigemDoComentario { get; private set; }
    public string? Obra { get; private set; }
    public string? Autor { get; private set; }
    public EstadoDePublicacao Publicacao { get; private set; }
    public EstadoDeModeracao Moderacao { get; private set; }
    public void DefinirProveniencia(MetodoDeCaptura metodo, OrigemDoConteudo origem, string? obra, string? autor)
    {
        if (!Enum.IsDefined(metodo) || !Enum.IsDefined(origem) || obra?.Length > 500 || autor?.Length > 500)
            throw new RegraDeDominioException("Proveniência inválida.");
        MetodoDeCaptura = metodo;
        OrigemDoComentario = origem;
        Obra = obra;
        Autor = autor;
        InvalidarPublicacao();
    }
    private void InvalidarPublicacao()
    {
        if (Publicacao != EstadoDePublicacao.Removido) Publicacao = EstadoDePublicacao.Privado;
    }
    public void PrepararPublicacao()
    {
        if (Publicacao == EstadoDePublicacao.Removido || Moderacao != EstadoDeModeracao.SemDenuncia)
            throw new RegraDeDominioException("Conteúdo sob moderação não pode ser republicado.");
        if (!string.IsNullOrWhiteSpace(TrechoOriginal) &&
            (string.IsNullOrWhiteSpace(Obra) || string.IsNullOrWhiteSpace(Autor) || (Pagina == null && string.IsNullOrWhiteSpace(Localizacao))))
            throw new RegraDeDominioException("Citação exige obra, autor e página ou localização.");
        Publicacao = EstadoDePublicacao.Publicavel;
    }
    public void Publicar(bool gateAberto)
    {
        if (!gateAberto) throw new RegraDeDominioException("Publicação social depende de revisão jurídica registrada.");
        PrepararPublicacao();
        Publicacao = EstadoDePublicacao.Publicado;
    }
    public void Denunciar()
    {
        if (Publicacao != EstadoDePublicacao.Publicado) throw new RegraDeDominioException("Apenas conteúdo público pode ser denunciado.");
        Moderacao = EstadoDeModeracao.Denunciado;
    }
    public void Retirar()
    {
        if (Publicacao != EstadoDePublicacao.Publicado) throw new RegraDeDominioException("Conteúdo não está publicado.");
        Publicacao = EstadoDePublicacao.Removido;
        Moderacao = EstadoDeModeracao.Retirado;
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
        InvalidarPublicacao();
        IdCapitulo = idCapitulo;
        Tipo = tipo;
        TrechoOriginal = trechoOriginal;
        Comentario = comentario;
        Pagina = pagina;
        Localizacao = localizacao;
    }
}
