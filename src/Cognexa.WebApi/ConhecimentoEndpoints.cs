using Cognexa.Application.Compartilhado;
using Cognexa.Application.Conhecimento;
namespace Cognexa.WebApi;

public static class ConhecimentoEndpoints
{
    public static void MapConhecimento(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/conhecimento").RequireAuthorization().AddEndpointFilter<ContaAtivaFilter>();
        group.MapGet("/aprendizados", (Guid? idLivro, Guid? idConceito, Guid? idAnotacao, int? limite, AprendizadoAppService service, CancellationToken ct) => service.ListarAsync(new(idLivro, idConceito, idAnotacao, limite ?? 100), ct));
        group.MapPost("/aprendizados", async (CriarAprendizadoDto dto, AprendizadoAppService service, CancellationToken ct) => { var result = await service.CriarAsync(dto, ct); return Results.Created($"/conhecimento/aprendizados/{result.Id}", result); });
        group.MapGet("/aprendizados/{id:guid}", (Guid id, AprendizadoAppService service, CancellationToken ct) => service.ObterAsync(id, ct));
        group.MapPut("/aprendizados/{id:guid}", (Guid id, AtualizarConteudoDto dto, AprendizadoAppService service, CancellationToken ct) => service.AtualizarAsync(id, dto, ct));
        group.MapDelete("/aprendizados/{id:guid}", async (Guid id, AprendizadoAppService service, CancellationToken ct) => { await service.RemoverAsync(id, ct); return Results.NoContent(); });
        group.MapGet("/conceitos", (ConceitoAppService service, CancellationToken ct) => service.ListarAsync(new SearchDto(), ct));
        group.MapPost("/conceitos", (SalvarConceitoDto dto, ConceitoAppService service, CancellationToken ct) => service.CriarAsync(dto, ct));
        group.MapGet("/conceitos/{id:guid}", (Guid id, ConceitoAppService service, CancellationToken ct) => service.ObterAsync(id, ct));
        group.MapPut("/conceitos/{id:guid}", (Guid id, SalvarConceitoDto dto, ConceitoAppService service, CancellationToken ct) => service.AtualizarAsync(id, dto, ct));
        group.MapDelete("/conceitos/{id:guid}", async (Guid id, ConceitoAppService service, CancellationToken ct) => { await service.RemoverAsync(id, ct); return Results.NoContent(); });
        group.MapGet("/aplicacoes", (Guid? idAprendizado, ConexoesAppService service, CancellationToken ct) => service.ListarAplicacoesAsync(idAprendizado, ct));
        group.MapPost("/aplicacoes", (SalvarAplicacaoPraticaDto dto, ConexoesAppService service, CancellationToken ct) => service.CriarAplicacaoAsync(dto, ct));
        group.MapPut("/aplicacoes/{id:guid}", (Guid id, AtualizarConteudoDto dto, ConexoesAppService service, CancellationToken ct) => service.AtualizarAplicacaoAsync(id, dto, ct));
        group.MapDelete("/aplicacoes/{id:guid}", async (Guid id, ConexoesAppService service, CancellationToken ct) => { await service.RemoverAplicacaoAsync(id, ct); return Results.NoContent(); });
        group.MapGet("/relacoes", (Guid? idConceito, ConexoesAppService service, CancellationToken ct) => service.ListarRelacoesAsync(idConceito, ct));
        group.MapPost("/relacoes", (CriarRelacaoDto dto, ConexoesAppService service, CancellationToken ct) => service.CriarRelacaoAsync(dto, ct));
        group.MapDelete("/relacoes/{id:guid}", async (Guid id, ConexoesAppService service, CancellationToken ct) => { await service.RemoverRelacaoAsync(id, ct); return Results.NoContent(); });
    }
}
