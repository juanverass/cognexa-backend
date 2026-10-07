using Cognexa.Application.Compartilhado;
using Cognexa.Domain.Conhecimento;
using Mapster;
using Cognexa.Domain.Compartilhado;
namespace Cognexa.Application.Conhecimento;

public record CriarAprendizadoDto(string Conteudo, Guid[] IdAnotacoes, Guid[] IdConceitos, OrigemDoConteudo Origem = OrigemDoConteudo.Manual);
public record AtualizarConteudoDto(string Conteudo);
public record FonteDoAprendizadoDto(Guid IdAnotacao, Guid IdLivro);
public record AprendizadoDto(Guid Id, string Conteudo, FonteDoAprendizadoDto[] Fontes, Guid[] IdConceitos, DateTimeOffset DataDeCriacao, OrigemDoConteudo Origem);
public record AprendizadoSearchDto(Guid? IdLivro = null, Guid? IdConceito = null, Guid? IdAnotacao = null, int Limite = 100) : SearchDto(Limite);
public record ConceitoDto(Guid Id, string Nome);
public record SalvarConceitoDto(string Nome);
public record AplicacaoPraticaDto(Guid Id, Guid IdAprendizado, string Descricao, OrigemDoConteudo Origem);
public record SalvarAplicacaoPraticaDto(Guid IdAprendizado, string Descricao, OrigemDoConteudo Origem = OrigemDoConteudo.Manual);
public record RelacaoEntreConceitosDto(Guid Id, Guid IdConceitoOrigem, Guid IdConceitoDestino, TipoDeRelacao Tipo);
public record CriarRelacaoDto(Guid IdConceitoOrigem, Guid IdConceitoDestino, TipoDeRelacao Tipo);
public sealed class MapeamentoConhecimento : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Aprendizado, AprendizadoDto>().Map(d => d.IdConceitos, s => s.Conceitos.Select(c => c.IdConceito).ToArray());
        config.NewConfig<FonteDoAprendizado, FonteDoAprendizadoDto>();
        config.NewConfig<Conceito, ConceitoDto>();
        config.NewConfig<AplicacaoPratica, AplicacaoPraticaDto>();
        config.NewConfig<RelacaoEntreConceitos, RelacaoEntreConceitosDto>();
    }
}
