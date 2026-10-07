using Cognexa.Application.Anotacoes;
using Cognexa.Domain.Anotacoes;
namespace Cognexa.WebApi;

public static class AnotacoesEndpoints
{
    public static void MapAnotacoes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/anotacoes").RequireAuthorization().AddEndpointFilter<ContaAtivaFilter>();
        group.MapGet("", (Guid? idLivro, TipoDeAnotacao? tipo, int? pagina, string? localizacao, Guid? idCapitulo, int? limite, AnotacaoAppService service, CancellationToken ct) => service.ListarAsync(new(idLivro, tipo, pagina, localizacao, idCapitulo, limite ?? 100), ct));
        group.MapPost("", async (SalvarAnotacaoDto dto, AnotacaoAppService service, CancellationToken ct) => { var result = await service.CriarAsync(dto, ct); return Results.Created($"/anotacoes/{result.Id}", result); });
        group.MapGet("/{id:guid}", (Guid id, AnotacaoAppService service, CancellationToken ct) => service.ObterAsync(id, ct));
        group.MapPut("/{id:guid}", (Guid id, SalvarAnotacaoDto dto, AnotacaoAppService service, CancellationToken ct) => service.AtualizarAsync(id, dto, ct));
        group.MapDelete("/{id:guid}", async (Guid id, AnotacaoAppService service, CancellationToken ct) => { await service.RemoverAsync(id, ct); return Results.NoContent(); });
    }
}
