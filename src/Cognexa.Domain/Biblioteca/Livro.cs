using Cognexa.Domain.Compartilhado;
namespace Cognexa.Domain.Biblioteca;

public sealed class Livro : EntidadeBase
{
    private readonly List<Autor> _autores = [];
    private Livro()
    {
    }
    public Livro(Guid idUsuario, string titulo, IEnumerable<string> autores, int? totalDePaginas = null, string? isbn = null, string? edicao = null, string? capa = null)
    {
        IdUsuario = Validacao.Id(idUsuario, "IdUsuario");
        Atualizar(titulo, autores, totalDePaginas, isbn, edicao, capa);
    }
    public Guid IdUsuario
    {
        get; private set;
    }
    public string Titulo { get; private set; } = "";
    public int? TotalDePaginas
    {
        get; private set;
    }
    public string? Isbn
    {
        get; private set;
    }
    public string? Edicao
    {
        get; private set;
    }
    public string? Capa
    {
        get; private set;
    }
    public IReadOnlyCollection<Autor> Autores => _autores.AsReadOnly();
    public void Atualizar(string titulo, IEnumerable<string> autores, int? totalDePaginas, string? isbn, string? edicao, string? capa)
    {
        if (totalDePaginas is <= 0)
            throw new RegraDeDominioException("Total de páginas deve ser positivo.");
        var nomes = autores?.Select(a => Validacao.Texto(a, "Autor", 200)).Distinct().ToArray() ?? [];
        if (nomes.Length is < 1 or > 30)
            throw new RegraDeDominioException("Informe entre 1 e 30 autores.");
        Titulo = Validacao.Texto(titulo, "Título", 300);
        TotalDePaginas = totalDePaginas;
        Isbn = isbn is null ? null : Validacao.Texto(isbn, "ISBN", 30);
        Edicao = edicao is null ? null : Validacao.Texto(edicao, "Edição", 100);
        Capa = capa is null ? null : Validacao.Texto(capa, "Capa", 2000);
        _autores.Clear();
        _autores.AddRange(nomes.Select(n => new Autor(Id, n)));
    }
}
public sealed class Autor : EntidadeBase
{
    private Autor()
    {
    }
    internal Autor(Guid idLivro, string nome)
    {
        IdLivro = idLivro;
        Nome = nome;
    }
    public Guid IdLivro
    {
        get; private set;
    }
    public string Nome { get; private set; } = "";
}
public sealed class Capitulo : EntidadeBase
{
    private Capitulo()
    {
    }
    public Capitulo(Guid idUsuario, Guid idLivro, string titulo, int ordem)
    {
        IdUsuario = Validacao.Id(idUsuario, "IdUsuario");
        IdLivro = Validacao.Id(idLivro, "IdLivro");
        Titulo = Validacao.Texto(titulo, "Título", 300);
        if (ordem < 1)
            throw new RegraDeDominioException("Ordem deve ser positiva.");
        Ordem = ordem;
    }
    public Guid IdUsuario
    {
        get; private set;
    }
    public Guid IdLivro
    {
        get; private set;
    }
    public string Titulo { get; private set; } = "";
    public int Ordem
    {
        get; private set;
    }
}
