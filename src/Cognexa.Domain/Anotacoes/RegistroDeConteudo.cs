using Cognexa.Domain.Compartilhado;
namespace Cognexa.Domain.Anotacoes;
public enum MotivoDeDenuncia { DireitosAutorais, AtribuicaoIncorreta, Outro }
public sealed class RegistroDeConteudo : EntidadeBase
{
    private RegistroDeConteudo() { }
    public RegistroDeConteudo(Guid idUsuario, Guid idLivro, int caracteres, int? pagina, DateTimeOffset agora, string chaveDaObra)
    { IdUsuario = idUsuario; IdLivro = idLivro; Caracteres = caracteres; Pagina = pagina; Data = agora; ChaveDaObra = chaveDaObra; }
    public Guid IdUsuario { get; private set; }
    public Guid IdLivro { get; private set; }
    public string ChaveDaObra { get; private set; } = "";
    public int Caracteres { get; private set; }
    public int? Pagina { get; private set; }
    public DateTimeOffset Data { get; private set; }
}
public sealed class Denuncia : EntidadeBase
{
    private Denuncia() { }
    public Denuncia(Guid idAnotacao, Guid idDenunciante, MotivoDeDenuncia motivo, DateTimeOffset agora)
    {
        if (!Enum.IsDefined(motivo)) throw new RegraDeDominioException("Motivo inválido.");
        IdAnotacao = idAnotacao; IdDenunciante = idDenunciante; Motivo = motivo; Data = agora;
    }
    public Guid IdAnotacao { get; private set; }
    public Guid IdDenunciante { get; private set; }
    public MotivoDeDenuncia Motivo { get; private set; }
    public DateTimeOffset Data { get; private set; }
}
// A auditoria preserva atribuição e transição, nunca o trecho/comentário.
public sealed class RegistroDePublicacao : EntidadeBase
{
    private RegistroDePublicacao() { }
    public RegistroDePublicacao(Anotacao anotacao, Guid ator, DateTimeOffset agora)
    {
        IdAnotacao = anotacao.Id; IdLivro = anotacao.IdLivro; IdAtor = ator;
        Obra = anotacao.Obra; Autor = anotacao.Autor;
        Publicacao = anotacao.Publicacao; Moderacao = anotacao.Moderacao; Data = agora;
    }
    public Guid IdAnotacao { get; private set; }
    public Guid IdLivro { get; private set; }
    public Guid IdAtor { get; private set; }
    public string? Obra { get; private set; }
    public string? Autor { get; private set; }
    public EstadoDePublicacao Publicacao { get; private set; }
    public EstadoDeModeracao Moderacao { get; private set; }
    public DateTimeOffset Data { get; private set; }
}
