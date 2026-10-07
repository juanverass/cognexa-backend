using Cognexa.Domain.Compartilhado;
namespace Cognexa.Domain.Conhecimento;

public sealed class Aprendizado : EntidadeBase
{
    private readonly List<FonteDoAprendizado> _fontes = [];
    private readonly List<ConceitoDoAprendizado> _conceitos = [];
    private Aprendizado()
    {
    }
    public Aprendizado(Guid idUsuario, string conteudo, IEnumerable<(Guid IdAnotacao, Guid IdLivro)> fontes, IEnumerable<Guid> conceitos, DateTimeOffset agora, OrigemDoConteudo origem = OrigemDoConteudo.Manual)
    {
        if (!Enum.IsDefined(origem))
            throw new RegraDeDominioException("Origem inválida.");
        Origem = origem;
        IdUsuario = Validacao.Id(idUsuario, "IdUsuario");
        Conteudo = Validacao.Texto(conteudo, "Conteúdo", 20000);
        DataDeCriacao = agora;
        var origens = fontes.Distinct().ToArray();
        if (origens.Length is < 1 or > 100)
            throw new RegraDeDominioException("Informe entre 1 e 100 fontes.");
        _fontes.AddRange(origens.Select(f => new FonteDoAprendizado(IdUsuario, Id, f.IdAnotacao, f.IdLivro)));
        _conceitos.AddRange(conceitos.Distinct().Select(c => new ConceitoDoAprendizado(IdUsuario, Id, c)));
        if (_conceitos.Count > 100)
            throw new RegraDeDominioException("Limite de 100 conceitos.");
    }
    public Guid IdUsuario
    {
        get; private set;
    }
    public OrigemDoConteudo Origem
    {
        get; private set;
    }
    public string Conteudo { get; private set; } = "";
    public DateTimeOffset DataDeCriacao
    {
        get; private set;
    }
    public IReadOnlyCollection<FonteDoAprendizado> Fontes => _fontes.AsReadOnly();
    public IReadOnlyCollection<ConceitoDoAprendizado> Conceitos => _conceitos.AsReadOnly();
    public void Atualizar(string conteudo) => Conteudo = Validacao.Texto(conteudo, "Conteúdo", 20000);
}
public sealed class FonteDoAprendizado : EntidadeBase
{
    private FonteDoAprendizado()
    {
    }
    internal FonteDoAprendizado(Guid idUsuario, Guid idAprendizado, Guid idAnotacao, Guid idLivro)
    {
        IdUsuario = idUsuario;
        IdAprendizado = idAprendizado;
        IdAnotacao = Validacao.Id(idAnotacao, "IdAnotacao");
        IdLivro = Validacao.Id(idLivro, "IdLivro");
    }
    public Guid IdUsuario
    {
        get; private set;
    }
    public Guid IdAprendizado
    {
        get; private set;
    }
    public Guid IdAnotacao
    {
        get; private set;
    }
    public Guid IdLivro
    {
        get; private set;
    }
}
public sealed class ConceitoDoAprendizado : EntidadeBase
{
    private ConceitoDoAprendizado()
    {
    }
    internal ConceitoDoAprendizado(Guid idUsuario, Guid idAprendizado, Guid idConceito)
    {
        IdUsuario = idUsuario;
        IdAprendizado = idAprendizado;
        IdConceito = Validacao.Id(idConceito, "IdConceito");
    }
    public Guid IdUsuario
    {
        get; private set;
    }
    public Guid IdAprendizado
    {
        get; private set;
    }
    public Guid IdConceito
    {
        get; private set;
    }
}
