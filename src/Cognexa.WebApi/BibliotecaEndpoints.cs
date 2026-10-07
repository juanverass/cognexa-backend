using Cognexa.Application.Biblioteca;
using Cognexa.Domain.Biblioteca;
namespace Cognexa.WebApi;

public static class BibliotecaEndpoints
{
    public static void MapBiblioteca(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/biblioteca").RequireAuthorization().AddEndpointFilter<ContaAtivaFilter>();
        group.MapGet("/livros", (string? titulo, int? limite, LivroAppService service, CancellationToken ct) => service.ListarAsync(new(titulo, limite ?? 100), ct));
        group.MapPost("/livros", async (SalvarLivroDto dto, LivroAppService service, CancellationToken ct) => { var result = await service.CriarAsync(dto, ct); return Results.Created($"/biblioteca/livros/{result.Id}", result); });
        group.MapGet("/livros/{id:guid}", (Guid id, LivroAppService service, CancellationToken ct) => service.ObterAsync(id, ct));
        group.MapPut("/livros/{id:guid}", (Guid id, SalvarLivroDto dto, LivroAppService service, CancellationToken ct) => service.AtualizarAsync(id, dto, ct));
        group.MapDelete("/livros/{id:guid}", async (Guid id, LivroAppService service, CancellationToken ct) => { await service.RemoverAsync(id, ct); return Results.NoContent(); });
        group.MapGet("/leituras", (StatusDaLeitura? status, LivroAppService service, CancellationToken ct) => service.ListarLeiturasAsync(status, ct));
        group.MapGet("/livros/{id:guid}/leitura", (Guid id, LivroAppService service, CancellationToken ct) => service.ObterLeituraAsync(id, ct));
        group.MapPut("/livros/{id:guid}/leitura", (Guid id, SalvarLeituraDto dto, LivroAppService service, CancellationToken ct) => service.AtualizarLeituraAsync(id, dto, ct));
        group.MapGet("/livros/{id:guid}/capitulos", (Guid id, LivroAppService service, CancellationToken ct) => service.ListarCapitulosAsync(id, ct));
        group.MapPost("/livros/{id:guid}/capitulos", (Guid id, CriarCapituloDto dto, LivroAppService service, CancellationToken ct) => service.CriarCapituloAsync(id, dto, ct));
    }
}
