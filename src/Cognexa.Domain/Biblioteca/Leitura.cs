using Cognexa.Domain.Compartilhado;
namespace Cognexa.Domain.Biblioteca;

public enum StatusDaLeitura
{
    QueroLer, Lendo, Pausado, Concluido, Abandonado
}
public sealed class Leitura : EntidadeBase
{
    private Leitura()
    {
    }
    public Leitura(Guid idUsuario, Guid idLivro, string? motivo = null)
    {
        IdUsuario = Validacao.Id(idUsuario, "IdUsuario");
        IdLivro = Validacao.Id(idLivro, "IdLivro");
        Motivo = motivo is null ? null : Validacao.Texto(motivo, "Motivo");
    }
    public Guid IdUsuario
    {
        get; private set;
    }
    public Guid IdLivro
    {
        get; private set;
    }
    public StatusDaLeitura Status
    {
        get; private set;
    }
    public int PaginaAtual
    {
        get; private set;
    }
    public string? Motivo
    {
        get; private set;
    }
    public DateTimeOffset? DataDeInicio
    {
        get; private set;
    }
    public DateTimeOffset? DataDeConclusao
    {
        get; private set;
    }
    public void Atualizar(StatusDaLeitura status, int paginaAtual, int? totalDePaginas, DateTimeOffset agora)
    {
        if (!Enum.IsDefined(status) || paginaAtual < 0 || totalDePaginas is <= 0 || (totalDePaginas.HasValue && paginaAtual > totalDePaginas))
            throw new RegraDeDominioException("Estado ou progresso de leitura inválido.");
        if (status == StatusDaLeitura.QueroLer && (PaginaAtual > 0 || DataDeInicio.HasValue || paginaAtual > 0))
            throw new ConflitoException("Leitura iniciada não pode voltar a QueroLer.");
        if (status is StatusDaLeitura.Pausado or StatusDaLeitura.Abandonado && DataDeInicio is null)
            throw new ConflitoException("Inicie a leitura antes de pausá-la ou abandoná-la.");
        if (status == StatusDaLeitura.Concluido && totalDePaginas.HasValue && paginaAtual != totalDePaginas)
            throw new RegraDeDominioException("Conclusão exige progresso igual ao total de páginas.");
        if (status is StatusDaLeitura.Lendo or StatusDaLeitura.Concluido)
            DataDeInicio ??= agora;
        DataDeConclusao = status == StatusDaLeitura.Concluido ? DataDeConclusao ?? agora : null;
        PaginaAtual = paginaAtual;
        Status = status;
    }
}
