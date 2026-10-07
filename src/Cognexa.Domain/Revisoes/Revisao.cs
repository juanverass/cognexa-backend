using Cognexa.Domain.Compartilhado;
namespace Cognexa.Domain.Revisoes;

public enum ResultadoDaRevisao
{
    Errou, Dificil, Bom, Facil
}
public enum NivelDeDominio
{
    Inicial, EmAprendizado, Consolidado, Dominado
}
public sealed class PerguntaDeRevisao : EntidadeBase
{
    private PerguntaDeRevisao()
    {
    }
    public PerguntaDeRevisao(Guid idUsuario, Guid idAprendizado, string pergunta, string respostaEsperada, OrigemDoConteudo origem = OrigemDoConteudo.Manual)
    {
        if (!Enum.IsDefined(origem))
            throw new RegraDeDominioException("Origem inválida.");
        Origem = origem;
        IdUsuario = Validacao.Id(idUsuario, "IdUsuario");
        IdAprendizado = Validacao.Id(idAprendizado, "IdAprendizado");
        Pergunta = Validacao.Texto(pergunta, "Pergunta", 4000);
        RespostaEsperada = Validacao.Texto(respostaEsperada, "Resposta esperada", 10000);
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
    public string Pergunta { get; private set; } = "";
    public string RespostaEsperada { get; private set; } = "";
}
public sealed class Revisao : EntidadeBase
{
    private Revisao()
    {
    }
    public Revisao(Guid idUsuario, Guid idPergunta, DateTimeOffset agora)
    {
        IdUsuario = Validacao.Id(idUsuario, "IdUsuario");
        IdPergunta = Validacao.Id(idPergunta, "IdPergunta");
        ProximaRevisao = agora;
    }
    public Guid IdUsuario
    {
        get; private set;
    }
    public Guid IdPergunta
    {
        get; private set;
    }
    public DateTimeOffset ProximaRevisao
    {
        get; private set;
    }
    public DateTimeOffset? UltimaRevisao
    {
        get; private set;
    }
    public int AcertosConsecutivos
    {
        get; private set;
    }
    public NivelDeDominio NivelDeDominio
    {
        get; private set;
    }
    public HistoricoDeRevisao Registrar(ResultadoDaRevisao resultado, string? resposta, DateTimeOffset agora, DateTimeOffset proxima)
    {
        if (!Enum.IsDefined(resultado) || proxima <= agora || resposta?.Length > 10000)
            throw new RegraDeDominioException("Resultado ou agendamento inválido.");
        if (agora < ProximaRevisao)
            throw new ConflitoException("Revisão ainda não está devida.");
        var anterior = NivelDeDominio;
        AcertosConsecutivos = resultado == ResultadoDaRevisao.Errou ? 0 : AcertosConsecutivos + 1;
        NivelDeDominio = AcertosConsecutivos switch
        {
            0 => NivelDeDominio.Inicial,
            < 3 => NivelDeDominio.EmAprendizado,
            < 6 => NivelDeDominio.Consolidado,
            _ => NivelDeDominio.Dominado
        };
        UltimaRevisao = agora;
        ProximaRevisao = proxima;
        return new HistoricoDeRevisao(IdUsuario, Id, resultado, resposta, agora, proxima, anterior, NivelDeDominio);
    }
}
public sealed class HistoricoDeRevisao : EntidadeBase
{
    private HistoricoDeRevisao()
    {
    }
    internal HistoricoDeRevisao(Guid idUsuario, Guid idRevisao, ResultadoDaRevisao resultado, string? resposta, DateTimeOffset data, DateTimeOffset proxima, NivelDeDominio anterior, NivelDeDominio novo)
    {
        IdUsuario = idUsuario;
        IdRevisao = idRevisao;
        Resultado = resultado;
        Resposta = resposta;
        Data = data;
        ProximaRevisao = proxima;
        NivelAnterior = anterior;
        NivelNovo = novo;
    }
    public Guid IdUsuario
    {
        get; private set;
    }
    public Guid IdRevisao
    {
        get; private set;
    }
    public ResultadoDaRevisao Resultado
    {
        get; private set;
    }
    public string? Resposta
    {
        get; private set;
    }
    public DateTimeOffset Data
    {
        get; private set;
    }
    public DateTimeOffset ProximaRevisao
    {
        get; private set;
    }
    public NivelDeDominio NivelAnterior
    {
        get; private set;
    }
    public NivelDeDominio NivelNovo
    {
        get; private set;
    }
}
