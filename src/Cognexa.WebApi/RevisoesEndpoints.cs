using Cognexa.Application.Revisoes;
namespace Cognexa.WebApi;

public static class RevisoesEndpoints
{
    public static void MapRevisoes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/revisoes").RequireAuthorization().AddEndpointFilter<ContaAtivaFilter>();
        group.MapPost("/perguntas", (CriarPerguntaDto dto, RevisaoAppService service, CancellationToken ct) => service.CriarAsync(dto, ct));
        group.MapGet("/perguntas/{id:guid}", (Guid id, RevisaoAppService service, CancellationToken ct) => service.ObterPerguntaAsync(id, ct));
        group.MapGet("/devidas", (Guid? idLivro, Guid? idConceito, int? limite, RevisaoAppService service, CancellationToken ct) => service.ListarDevidasAsync(idLivro, idConceito, limite ?? 100, ct));
        group.MapGet("/{id:guid}", (Guid id, RevisaoAppService service, CancellationToken ct) => service.ObterAsync(id, ct));
        group.MapPost("/{id:guid}/respostas", (Guid id, RegistrarRevisaoDto dto, RevisaoAppService service, CancellationToken ct) => service.RegistrarAsync(id, dto, ct));
        group.MapGet("/{id:guid}/historico", (Guid id, int? pagina, RevisaoAppService service, CancellationToken ct) => service.ListarHistoricoAsync(id, pagina ?? 1, ct));
    }
}
