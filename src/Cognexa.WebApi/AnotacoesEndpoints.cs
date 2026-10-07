using Cognexa.Application.Anotacoes;
using Cognexa.Domain.Anotacoes;
namespace Cognexa.WebApi;

public static class AnotacoesEndpoints
{
    public static void MapAnotacoes(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/anotacoes").RequireAuthorization().AddEndpointFilter<ContaAtivaFilter>();
        group.MapPost("/ocr", async (HttpRequest request, OcrAppService service, CancellationToken ct) =>
        {
            if (request.ContentLength is null or > 5000000 or < 8)
                throw new Cognexa.Domain.Compartilhado.RegraDeDominioException("Informe imagem de até 5 MB e Content-Length.");
            var imagem = new byte[(int)request.ContentLength.Value];
            try
            {
                await request.Body.ReadExactlyAsync(imagem, ct);
                return Results.Ok(await service.ExtrairAsync(imagem, request.ContentType ?? "", ct));
            }
            finally { System.Security.Cryptography.CryptographicOperations.ZeroMemory(imagem); }
        });
        group.MapPost("/ocr/confirmar", (ConfirmarOcrDto dto, OcrAppService service, CancellationToken ct) => service.ConfirmarAsync(dto, ct));
        group.MapPost("/{id:guid}/preparar-publicacao", async (Guid id, PublicacaoAppService service, CancellationToken ct) => { await service.PrepararAsync(id, false, ct); return Results.NoContent(); });
        group.MapPost("/{id:guid}/publicar", async (Guid id, PublicacaoAppService service, CancellationToken ct) => { await service.PrepararAsync(id, true, ct); return Results.NoContent(); });
        group.MapPost("/{id:guid}/retirar", async (Guid id, PublicacaoAppService service, CancellationToken ct) => { await service.RetirarAsync(id, ct); return Results.NoContent(); });
        routes.MapGet("/publicacoes/{id:guid}", (Guid id, PublicacaoAppService service, CancellationToken ct) => service.ObterAsync(id, ct));
        routes.MapPost("/publicacoes/{id:guid}/denuncias", async (Guid id, DenunciarDto dto, PublicacaoAppService service, CancellationToken ct) => { await service.DenunciarAsync(id, dto.Motivo, ct); return Results.NoContent(); }).RequireAuthorization().AddEndpointFilter<ContaAtivaFilter>();
        group.MapGet("", (Guid? idLivro, TipoDeAnotacao? tipo, int? pagina, string? localizacao, Guid? idCapitulo, int? limite, AnotacaoAppService service, CancellationToken ct) => service.ListarAsync(new(idLivro, tipo, pagina, localizacao, idCapitulo, limite ?? 100), ct));
        group.MapPost("", async (SalvarAnotacaoDto dto, AnotacaoAppService service, CancellationToken ct) => { var result = await service.CriarAsync(dto, ct); return Results.Created($"/anotacoes/{result.Id}", result); });
        group.MapGet("/{id:guid}", (Guid id, AnotacaoAppService service, CancellationToken ct) => service.ObterAsync(id, ct));
        group.MapPut("/{id:guid}", (Guid id, SalvarAnotacaoDto dto, AnotacaoAppService service, CancellationToken ct) => service.AtualizarAsync(id, dto, ct));
        group.MapDelete("/{id:guid}", async (Guid id, AnotacaoAppService service, CancellationToken ct) => { await service.RemoverAsync(id, ct); return Results.NoContent(); });
    }
}

public record DenunciarDto(MotivoDeDenuncia Motivo);
